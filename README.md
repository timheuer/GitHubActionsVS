[![Build](https://github.com/timheuer/GitHubActionsVS/actions/workflows/_build.yaml/badge.svg)](https://github.com/timheuer/GitHubActionsVS/actions/workflows/_build.yaml)
[![GitHub last commit](https://img.shields.io/github/last-commit/timheuer/GitHubActionsVS)](https://github.com/timheuer/GitHubActionsVS/)
[![VS Marketplace Badge](https://img.shields.io/visual-studio-marketplace/v/timheuer.GitHubActionsVS?label=VS%20Marketplace&color=purple&logo=visualstudio)](https://marketplace.visualstudio.com/items?itemName=TimHeuer.GitHubActionsVS)

<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-3-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

# GitHub Actions for Visual Studio

The GitHub Actions extension lets you manage your workflows, view the workflow run history, and edit GitHub secrets.

<img width="668" alt="image" src="https://github.com/timheuer/GitHubActionsVS/assets/4821/aaa07dd3-512e-4a9b-a768-087372bf5e34">


## Features

This extension mainly serves to provide a quick way to see the GitHub Actions for your open solution if identified as a GitHub.com repo. To view these, either right click on the solution or project in Solution Explorer and click "GitHub Actions" from the menu:

![image](https://github.com/timheuer/GitHubActionsVS/assets/4821/ab61f0b9-d82d-41f4-bc29-cfc7507fea7f)

If an active solution exists and it is both a git and GitHub.com repository, the window will start querying the repository for Actions information on runs and secrets. A progress bar will be shown then you can expand to see the results.

### View workflow run history

To view the history simply select a run and navigate through the tree view to see details. You can double-click on a leaf node to launch to the log point on the repo to view the rich log output.

If you close and open a new project the window will be refreshed to represent the current state.

Based on your settings you can enable 'polling' of active running workflows that are not in the `completed` status. This will refresh the Current Branch workflow runs until the state is completed.

#### Limit run count retrieval
By default a maximum of last 10 runs are retrieved. You can change this in the `Tools...Options` of Visual Studio and set an integer value.

![image](https://github.com/timheuer/GitHubActionsVS/assets/4821/661f28cb-b906-476f-ae57-accfbdd63f1f)

#### Trigger a Workflow
If your Workflows enable a dispatch capability you can trigger to run a workflow directly from Visual Studio:

![image](https://github.com/timheuer/GitHubActionsVS/assets/4821/f90697f8-a9e7-4d76-bcad-35d5c4a7ff58)

#### Manually refresh
You can manually refresh the view by clicking the refresh icon in the toolbar:

![image](https://github.com/timheuer/GitHubActionsVS/assets/4821/865d424d-29e1-40e8-96c4-1eeffb458682)

### Edit GitHub secrets
The limitation currently is this lists and enables editing of Repository-level secrets (not org or deployment environments yet).

To add a secret right-click on the Repository Secrets node and select `Add Secret`

![image](https://github.com/timheuer/GitHubActionsVS/assets/4821/264acf44-509b-4442-a9b9-80f93ff5cad5)

This will launch a modal dialog to add the repository secret. This is the same for edit (right-click on an existing secret) which will enable you to edit an existing one or delete.

![image](https://github.com/timheuer/GitHubActionsVS/assets/4821/ba12fe8b-9f33-46b7-a4a4-1ae343a0ce34)


## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://davidpine.net"><img src="https://avatars.githubusercontent.com/u/7679720?v=4?s=100" width="100px;" alt="David Pine"/><br /><sub><b>David Pine</b></sub></a><br /><a href="https://github.com/timheuer/GitHubActionsVS/commits?author=IEvangelist" title="Code">💻</a> <a href="https://github.com/timheuer/GitHubActionsVS/commits?author=IEvangelist" title="Documentation">📖</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://timheuer.com/blog/"><img src="https://avatars.githubusercontent.com/u/4821?v=4?s=100" width="100px;" alt="Tim Heuer"/><br /><sub><b>Tim Heuer</b></sub></a><br /><a href="https://github.com/timheuer/GitHubActionsVS/commits?author=timheuer" title="Code">💻</a> <a href="https://github.com/timheuer/GitHubActionsVS/commits?author=timheuer" title="Documentation">📖</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/zlatanov"><img src="https://avatars.githubusercontent.com/u/2470527?v=4?s=100" width="100px;" alt="Ivan Zlatanov"/><br /><sub><b>Ivan Zlatanov</b></sub></a><br /><a href="https://github.com/timheuer/GitHubActionsVS/commits?author=zlatanov" title="Code">💻</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

## Requirements

Visual Studio 2022 17.6 or later is required to use this extension.  Additionally since GitHub Actions is obviously a feature of GitHub, you will need to be attached to an active GitHub.com repository.

## Code of Conduct

This project has adopted the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct). For more information see the Code of Conduct itself or contact project maintainers with any additional questions or comments or to report a violation.
