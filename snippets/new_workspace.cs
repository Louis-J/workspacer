// Add this to the top of your configuration file.
using System.Linq;

// Register Alt+Shift+N to create a new workspace
context.Keybinds.Subscribe(mod | KeyModifiers.LShift, Keys.N, () => {
	var num_workspaces = context.WorkspaceContainer.GetAllWorkspaces().Count();
	context.WorkspaceContainer.CreateWorkspace((num_workspaces + 1).ToString());
}, "Create new workspace");
