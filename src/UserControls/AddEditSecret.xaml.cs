using System.Text.RegularExpressions;
using System.Windows;
using resx = GitHubActionsVS.Resources.UIStrings;

namespace GitHubActionsVS.UserControls;
/// <summary>
/// Interaction logic for AddEditSecret.xaml
/// </summary>
public partial class AddEditSecret : Window
{
    public AddEditSecret(string secretName)
    {
        InitializeComponent();
        txtName.Text = secretName;
        txtName.IsEnabled = string.IsNullOrWhiteSpace(secretName);
        Title = string.IsNullOrWhiteSpace(secretName) ? resx.ADD_SECRET : resx.EDIT_SECRET;
        btnCreate.Content = string.IsNullOrWhiteSpace(secretName) ? resx.BUTTON_SAVE : resx.BUTTON_UPDATE;
    }

    public string SecretName => txtName.Text.Trim();
    public string SecretValue => txtSecret.Text.Trim();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Secret names can only contain alphanumeric characters ([a-z], [A-Z], [0-9]) or underscores (_). Spaces are not allowed. Must start with a letter ([a-z], [A-Z]) or underscores (_).
        Regex rx = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$");
        if (rx.IsMatch(txtName.Text))
        {
            DialogResult = true;
            Close();
        }
        else
        {
            Community.VisualStudio.Toolkit.MessageBox mb = new();
            mb.ShowError("Secret names can only contain alphanumeric characters ([a-z], [A-Z], [0-9]) or underscores (_). Spaces are not allowed. Must start with a letter ([a-z], [A-Z]) or underscores (_).", "Invalid Secret Name");
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
