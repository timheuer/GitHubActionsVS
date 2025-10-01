using GitHubActionsVS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GitHubActionsVS.UserControls
{
    public partial class WorkflowInputsDialog : Window
    {
        private readonly Dictionary<string, FrameworkElement> _controls = new(StringComparer.Ordinal);
        private readonly List<InputMetadata> _metas;

        public WorkflowInputsDialog(string workflowName, List<InputMetadata> metas)
        {
            InitializeComponent();

            _metas = metas ?? [];

            Title = $"Workflow Inputs: {workflowName}";

            for (int i = 0; i < _metas.Count; i++)
            {
                InputsPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var meta = _metas[i];

                string labelText = !string.IsNullOrWhiteSpace(meta.Description) ? meta.Description! : meta.Name;
                if (meta.Required) labelText += " *";

                var label = new TextBlock
                {
                    Text = labelText,
                    Margin = new Thickness(0, 6, 12, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(label, i);
                Grid.SetColumn(label, 0);
                InputsPanel.Children.Add(label);

                bool hasYamlOptions = meta.Options is { Length: > 0 };
                FrameworkElement control =
                    hasYamlOptions
                        ? new ComboBox
                        {
                            ItemsSource = meta.Options!,
                            SelectedItem = (meta.Options!.Contains(meta.Default ?? "", StringComparer.Ordinal)
                                            ? meta.Default
                                            : meta.Options!.FirstOrDefault()),
                            Margin = new Thickness(0, 4, 0, 4),
                            MinWidth = 220
                        }
                    : meta.Type switch
                    {
                        "boolean" => new CheckBox
                        {
                            IsChecked = bool.TryParse(meta.Default, out var b) && b,
                            Margin = new Thickness(0, 4, 0, 4)
                        },
                        "number" => CreateNumericTextBox(meta.Default),
                        _ => new TextBox
                        {
                            Text = meta.Default ?? string.Empty,
                            Margin = new Thickness(0, 4, 0, 4),
                            Width = 300
                        }
                    };

                control.Tag = meta;
                Grid.SetRow(control, i);
                Grid.SetColumn(control, 1);
                InputsPanel.Children.Add(control);

                _controls[meta.Name] = control;
            }
        }

        private static TextBox CreateNumericTextBox(string? defaultValue)
        {
            var tb = new TextBox
            {
                Text = defaultValue ?? string.Empty,
                Margin = new Thickness(0, 4, 0, 4),
                Width = 140
            };

            tb.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsNumericInput(e.Text);
            };

            DataObject.AddPastingHandler(tb, (s, e) =>
            {
                if (e.DataObject.GetDataPresent(DataFormats.Text))
                {
                    var text = (string)e.DataObject.GetData(DataFormats.Text);
                    if (!IsNumericInput(text))
                        e.CancelCommand();
                }
                else e.CancelCommand();
            });

            return tb;
        }

        private static bool IsNumericInput(string text)
        {
            foreach (char c in text)
            {
                if (!(char.IsDigit(c) || c == '-' || c == '.'))
                    return false;
            }
            return true;
        }

        public Dictionary<string, string> GetInputValues()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var kvp in _controls)
            {
                var name = kvp.Key;
                var control = kvp.Value;
                string value;

                if (control is TextBox tb)
                {
                    value = tb.Text ?? string.Empty;
                }
                else if (control is CheckBox cb)
                {
                    value = (cb.IsChecked == true) ? "true" : "false";
                }
                else if (control is ComboBox combo)
                {
                    var selectedValue = combo.SelectedValue?.ToString();
                    var selectedItem = combo.SelectedItem is ComboBoxItem cbi ? cbi.Content?.ToString()
                                        : combo.SelectedItem?.ToString();
                    value = !string.IsNullOrWhiteSpace(selectedValue) ? selectedValue!
                        : !string.IsNullOrWhiteSpace(selectedItem) ? selectedItem!
                        : (combo.Text ?? string.Empty);
                }
                else
                {
                    value = string.Empty;
                }

                values[name] = value;
            }

            return values;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var kvp in _controls)
            {
                var control = kvp.Value;
                var meta = (InputMetadata)control.Tag;
                if (!meta.Required) continue;

                string display = !string.IsNullOrWhiteSpace(meta.Description) ? meta.Description! : meta.Name;

                bool isMissing = false;

                if (control is TextBox tb)
                {
                    isMissing = string.IsNullOrWhiteSpace(tb.Text);
                }
                else if (control is CheckBox)
                {
                    isMissing = false;
                }
                else if (control is ComboBox cb)
                {
                    string chosen = null;

                    if (cb.SelectedValue != null)
                        chosen = cb.SelectedValue.ToString();
                    else if (cb.SelectedItem is ComboBoxItem cbi && cbi.Content != null)
                        chosen = cbi.Content.ToString();
                    else if (cb.SelectedItem != null)
                        chosen = cb.SelectedItem.ToString();

                    if (string.IsNullOrWhiteSpace(chosen))
                        chosen = cb.Text;

                    isMissing = string.IsNullOrWhiteSpace(chosen);
                }
                else
                {
                    isMissing = false;
                }

                if (isMissing)
                {
                    System.Windows.MessageBox.Show(
                        $"{display} is required.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    control.Focus();
                    control?.BringIntoView();
                    return;
                }
            }

            DialogResult = true;
            Close();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
