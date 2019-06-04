﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Docking;
using Lunar.Core;
using Lunar.Core.Utilities.Logic;

namespace Lunar.Editor.Controls
{
    public partial class DockProject : DarkToolWindow
    {
        private Project _project;

        #region Constructor Region

        public DockProject()
        {
            InitializeComponent();

            this.treeProject.MultiSelect = false;
            this.treeProject.MouseDoubleClick += TreeProject_MouseDoubleClick;

            this.treeProject.MouseDown += TreeProject_MouseDown;
            
        }

        #endregion

        private DarkTreeNode GetNodeAt(Point point)
        {
            // Let's get nasty with an inline recursive function ;)
            Func<DarkTreeNode, DarkTreeNode> recFind = null;
            // We have to use this little trick of splitting the defining the variable and assigning its value in order to allow the function to reference itself.
            recFind = new Func<DarkTreeNode, DarkTreeNode>((curNode) =>
            {
                if (this.treeProject.GetNodeFullRowArea(curNode).Contains(point))
                    return curNode;

                foreach (var node in curNode.Nodes)
                {
                    var foundNode = recFind(node);

                    if (foundNode != null)
                        return foundNode;
                }

                return null;
            });

            // We can have many top-most nodes, so we have to start by looping through them
            // and initiating the recursive recFind algorithm on all of them.
            foreach (var node in this.treeProject.Nodes)
            {
                var foundNode = recFind(node);

                if (foundNode != null)
                    return foundNode;
            }

            // If we reach this point... we're SOL finding a matching node.
            return null;
        }

        private void TreeProject_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var selectedNode = this.GetNodeAt(e.Location);

            if (selectedNode == null) return;

            this.treeProject.SelectedNodes.Clear();
            this.treeProject.SelectedNodes.Add(selectedNode);

            if (selectedNode.Tag?.ToString().Contains(EngineConstants.NPC_FILE_EXT) == true)
            {
                this.npcExplorerMenu.Show(this.treeProject, e.Location);
            }
            else
            {
                this.projectExplorerMenu.Show(this.treeProject, e.Location);
            }
        }

        private void TreeProject_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeProject.SelectedNodes.Count > 0)
            {
                if (treeProject.SelectedNodes[0].Tag is FileInfo info)
                {
                    this.FileSelected?.Invoke(this, new FileEventArgs(info));
                }
                else
                {
                    (treeProject.SelectedNodes[0].Tag as Action<DarkTreeNode>)?.Invoke(treeProject.SelectedNodes[0].ParentNode);
                }
            }
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var createScriptDialog = new CreateScriptDialog();

            if (createScriptDialog.ShowDialog() == DialogResult.OK)
            {
                string directory = (this.treeProject.SelectedNodes[0].Tag as FileInfo).DirectoryName + "./scripts/";

                // Make sure the .scripts directory exists.
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var filePath = directory + createScriptDialog.ScriptName + ".py";

                var scriptFile = _project.AddScript(this.GetNextAvailableFilename(filePath));
                this.FileCreated?.Invoke(this, new FileEventArgs(scriptFile));

                var fileNode = new DarkTreeNode(scriptFile.Name)
                {
                    Tag = scriptFile,
                    Icon = Icons.document_16xLG,
                };

                this.treeProject.SelectedNodes[0].Nodes.Add(fileNode);
            }
        }

        private DarkTreeNode BuildAnimationTree()
        {
            var animationPathNode = new DarkTreeNode("Animations")
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open
            };

            foreach (var animationFile in _project.AnimationFiles)
            {
                var fileNode = new DarkTreeNode(animationFile.Name)
                {
                    Tag = animationFile,
                    Icon = Icons.document_16xLG,
                };
                animationPathNode.Nodes.Add(fileNode);
            }

            var addNode = new DarkTreeNode("Add Animation")
            {
                Icon = Icons.Plus,
                Tag = (Action<DarkTreeNode>)((node) =>
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.RestoreDirectory = true;
                        dialog.InitialDirectory = _project.ServerWorldDirectory.FullName + @"\Animations";
                        dialog.Filter = $@"Lunar Engine Animation Files (*{EngineConstants.ANIM_FILE_EXT})|*{EngineConstants.ANIM_FILE_EXT}";
                        dialog.DefaultExt = EngineConstants.ANIM_FILE_EXT;
                        dialog.AddExtension = true;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string path = dialog.FileName;

                            var file = _project.AddAnimation(path);

                            this.FileCreated?.Invoke(this, new FileEventArgs(file));
                        }
                    }
                })
            };

            animationPathNode.Nodes.Add(addNode);

            return animationPathNode;
        }

        private DarkTreeNode BuildScriptTree()
        {
            var scriptPathNode = new DarkTreeNode("Scripts")
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open
            };

            foreach (var scriptFile in _project.ScriptFiles)
            {
                var fileNode = new DarkTreeNode(scriptFile.Name)
                {
                    Tag = scriptFile,
                    Icon = Icons.document_16xLG,
                };
                scriptPathNode.Nodes.Add(fileNode);
            }

            var addNode = new DarkTreeNode("Add Script")
            {
                Icon = Icons.Plus,
                Tag = (Action<DarkTreeNode>)((node) =>
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.RestoreDirectory = true;
                        dialog.InitialDirectory = _project.ServerWorldDirectory.FullName + @"\Scripts";
                        dialog.Filter = $@"Python Script Files (*{EngineConstants.SCRIPT_FILE_EXT})|*{EngineConstants.SCRIPT_FILE_EXT}";
                        dialog.DefaultExt = EngineConstants.SCRIPT_FILE_EXT;
                        dialog.AddExtension = true;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string path = dialog.FileName;

                            var file = _project.AddScript(path);

                            this.FileCreated?.Invoke(this, new FileEventArgs(file));
                        }
                    }
                })
            };

            scriptPathNode.Nodes.Add(addNode);

            return scriptPathNode;
        }

        private DarkTreeNode BuildMapTree()
        {
            var mapPathNode = new DarkTreeNode("Maps")
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open
            };

            foreach (var mapFile in _project.MapFiles)
            {
                var fileNode = new DarkTreeNode(mapFile.Name)
                {
                    Tag = mapFile,
                    Icon = Icons.document_16xLG,
                };
                mapPathNode.Nodes.Add(fileNode);
            }

            var addNode = new DarkTreeNode("Add Map")
            {
                Icon = Icons.Plus,
                Tag = (Action<DarkTreeNode>) ((node) =>
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.RestoreDirectory = true;
                        dialog.InitialDirectory = _project.ServerWorldDirectory.FullName + @"\Maps";
                        dialog.Filter = $@"Lunar Engine Item Files (*{EngineConstants.MAP_FILE_EXT})|*{EngineConstants.MAP_FILE_EXT}";
                        dialog.DefaultExt = EngineConstants.MAP_FILE_EXT;
                        dialog.AddExtension = true;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string path = dialog.FileName;

                            var file = _project.AddMap(path);

                            this.FileCreated?.Invoke(this, new FileEventArgs(file));
                        }
                    }
                })
            };
            
            mapPathNode.Nodes.Add(addNode);

            return mapPathNode;
        }

        private DarkTreeNode BuildItemTree()
        {
            var itemPathNode = new DarkTreeNode("Items")
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open
            };

            foreach (var itemFile in _project.ItemFiles)
            {
                var fileNode = new DarkTreeNode(itemFile.Name)
                {
                    Tag = itemFile,
                    Icon = Icons.document_16xLG,
                };
                itemPathNode.Nodes.Add(fileNode);
            }

            var addNode = new DarkTreeNode("Add Item")
            {
                Icon = Icons.Plus,
                Tag = (Action<DarkTreeNode>) ((node) =>
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.InitialDirectory = _project.ServerWorldDirectory.FullName + @"\Items";
                        dialog.RestoreDirectory = true;
                        dialog.Filter = $@"Lunar Engine Item Files (*{EngineConstants.ITEM_FILE_EXT})|*{EngineConstants.ITEM_FILE_EXT}";
                        dialog.DefaultExt = EngineConstants.ITEM_FILE_EXT;
                        dialog.AddExtension = true;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string path = dialog.FileName;

                            var file = _project.AddItem(path);

                            this.FileCreated?.Invoke(this, new FileEventArgs(file));
                        }
                    }
                })
            };


            itemPathNode.Nodes.Add(addNode);

            return itemPathNode;
        }

        private DarkTreeNode BuildNPCTree()
        {
            var npcPathNode = new DarkTreeNode("Npcs")
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open
            };

            foreach (var npcFile in _project.NPCFiles)
            {
                var fileNode = new DarkTreeNode(npcFile.Name)
                {
                    Tag = npcFile,
                    Icon = Icons.document_16xLG,
                };
                npcPathNode.Nodes.Add(fileNode);
            }

            var addNode = new DarkTreeNode("Add NPC")
            {
                Icon = Icons.Plus,
                Tag = (Action<DarkTreeNode>)((node) =>
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.InitialDirectory = _project.ServerWorldDirectory.FullName + @"\Npcs";
                        dialog.RestoreDirectory = true;
                        dialog.Filter = $@"Lunar Engine NPC Files (*{EngineConstants.NPC_FILE_EXT})|*{EngineConstants.NPC_FILE_EXT}";
                        dialog.DefaultExt = EngineConstants.NPC_FILE_EXT;
                        dialog.AddExtension = true;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string path = dialog.FileName;

                            var file = _project.AddNPC(path);

                            this.FileCreated?.Invoke(this, new FileEventArgs(file));
                        }
                    }
                })
            };

            npcPathNode.Nodes.Add(addNode);

            return npcPathNode;
        }

        private DarkTreeNode InitalizeProjectTree()
        {
            DarkTreeNode projectTreeNode = new DarkTreeNode("Game Data")
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open
            };

            projectTreeNode.Nodes.Add(this.BuildMapTree());
            projectTreeNode.Nodes.Add(this.BuildItemTree());
            projectTreeNode.Nodes.Add(this.BuildAnimationTree());
            projectTreeNode.Nodes.Add(this.BuildNPCTree());
            projectTreeNode.Nodes.Add(this.BuildScriptTree());

            return projectTreeNode;
        }

        public void InitalizeFromProject(Project project)
        {
            _project = project;

            _project.ItemAdded += ProjectOnItemAdded;
            _project.ItemDeleted += ProjectOnItemDeleted;
            _project.ItemChanged += ProjectOnItemChanged;

            _project.NPCAdded += ProjectOnNpcAdded;
            _project.NPCDeleted += ProjectOnNpcDeleted;
            _project.NPCChanged += ProjectOnNpcChanged;

            _project.MapAdded += ProjectOnMapAdded;
            _project.MapDeleted += ProjectOnMapDeleted;
            _project.MapChanged += ProjectOnMapChanged;

            _project.AnimationAdded += ProjectOnAnimationAdded;
            _project.AnimationDeleted += ProjectOnAnimationDeleted;
            _project.AnimationChanged += ProjectOnAnimationChanged;

            _project.ScriptAdded += ProjectOnScriptAdded;
            _project.ScriptDeleted += ProjectOnScriptDeleted;
            _project.ScriptChanged += ProjectOnScriptChanged;


            treeProject.Nodes.Clear();

            var node = new DarkTreeNode(_project.GameName)
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open
            };

            node.Nodes.Add(this.InitalizeProjectTree());

            treeProject.Nodes.Add(node);
        }

        private void ProjectOnScriptChanged(object sender, GameFileChangedEventArgs args)
        {
            var nodeToChange = treeProject.FindNode($"Default\\Game Data\\Scripts\\{args.OldFile.Name}");
            nodeToChange.Tag = args.NewFile;
            this.UpdateNode(nodeToChange, args.NewFile.Name);

            this.FileChanged?.Invoke(this, args);
        }

        private void ProjectOnScriptDeleted(object sender, FileEventArgs args)
        {
            var nodeToDelete = treeProject.FindNode($"Default\\Game Data\\Scripts\\{args.File.Name}");

            this.FileRemoved?.Invoke(this, new FileEventArgs(args.File));

            nodeToDelete?.ParentNode.Nodes.Remove(nodeToDelete);
        }

        private void ProjectOnScriptAdded(object sender, FileEventArgs args)
        {
            // Make sure this isn't a content script
            if (!Helpers.IsSubDirectoryOf(args.File.DirectoryName, new DirectoryInfo(_project.ServerRootDirectory + "/Scripts/").FullName))
                return;

            DarkTreeNode fileNode = new DarkTreeNode(args.File.Name)
            {
                Icon = Icons.document_16xLG,
                Tag = args.File
            };

            var addNode = treeProject.FindNode($"Default\\Game Data\\Scripts\\Add Script");

            var scriptsNode = treeProject.FindNode($"Default\\Game Data\\Scripts");
            scriptsNode.Nodes.Remove(addNode);

            scriptsNode.Nodes.Add(fileNode);
            scriptsNode.Nodes.Add(addNode);
        }


        private void ProjectOnItemChanged(object sender, GameFileChangedEventArgs args)
        {
            var nodeToChange = treeProject.FindNode($"Default\\Game Data\\Items\\{args.OldFile.Name}");
            nodeToChange.Tag = args.NewFile;
            this.UpdateNode(nodeToChange, args.NewFile.Name);

            this.FileChanged?.Invoke(this, args);
        }

        private void ProjectOnAnimationChanged(object sender, GameFileChangedEventArgs args)
        {
            var nodeToChange = treeProject.FindNode($"Default\\Game Data\\Animations\\{args.OldFile.Name}");
            nodeToChange.Tag = args.NewFile;
            this.UpdateNode(nodeToChange, args.NewFile.Name);

            this.FileChanged?.Invoke(this, args);
        }

        private void ProjectOnMapChanged(object sender, GameFileChangedEventArgs args)
        {
            var nodeToChange = treeProject.FindNode($"Default\\Game Data\\Maps\\{args.OldFile.Name}");
            nodeToChange.Tag = args.NewFile;
            this.UpdateNode(nodeToChange, args.NewFile.Name);

            this.FileChanged?.Invoke(this, args);
        }

        private void ProjectOnNpcChanged(object sender, GameFileChangedEventArgs args)
        {
            var nodeToChange = treeProject.FindNode($"Default\\Game Data\\Npcs\\{args.OldFile.Name}");
            nodeToChange.Tag = args.NewFile;
            this.UpdateNode(nodeToChange, args.NewFile.Name);

            this.FileChanged?.Invoke(this, args);
        }

        private void UpdateNode(DarkTreeNode nodeToChange, string newName)
        {
            nodeToChange.Text = newName;

            // ANOTHER FUCKING HACK, THANKS DarkUI
            var parentNode = nodeToChange.ParentNode;
            int oldIndex = parentNode.Nodes.IndexOf(nodeToChange);
            parentNode.Nodes.Remove(nodeToChange);
            parentNode.Nodes.Insert(oldIndex, nodeToChange);

            parentNode.Nodes.Add(new DarkTreeNode());
            parentNode.Nodes.RemoveAt(parentNode.Nodes.Count - 1);
        }

        private void ProjectOnAnimationDeleted(object sender, FileEventArgs args)
        {
            var nodeToDelete = treeProject.FindNode($"Default\\Game Data\\Animations\\{args.File.Name}");

            this.FileRemoved?.Invoke(this, new FileEventArgs(args.File));

            nodeToDelete?.ParentNode.Nodes.Remove(nodeToDelete);
        }

        private void ProjectOnAnimationAdded(object sender, FileEventArgs args)
        {
            DarkTreeNode fileNode = new DarkTreeNode(args.File.Name)
            {
                Icon = Icons.document_16xLG,
                Tag = args.File
            };

            var addNode = treeProject.FindNode($"Default\\Game Data\\Animations\\Add Animation");
            treeProject.Nodes.Remove(addNode);

            var animationsNode = treeProject.FindNode($"Default\\Game Data\\Animations");

            animationsNode.Nodes.Add(fileNode);
            animationsNode.Nodes.Add(addNode);
        }

        private void ProjectOnMapDeleted(object sender, FileEventArgs args)
        {
            var nodeToDelete = treeProject.FindNode($"Default\\Game Data\\Maps\\{args.File.Name}");

            this.FileRemoved?.Invoke(this, new FileEventArgs(args.File));

            nodeToDelete?.ParentNode.Nodes.Remove(nodeToDelete);
        }

        private void ProjectOnMapAdded(object sender, FileEventArgs args)
        {
            DarkTreeNode fileNode = new DarkTreeNode(args.File.Name)
            {
                Icon = Icons.document_16xLG,
                Tag = args.File
            };

            var mapsNode = treeProject.FindNode($"Default\\Game Data\\Maps");

            var addNode = treeProject.FindNode($"Default\\Game Data\\Maps\\Add Map");
            mapsNode.Nodes.Remove(addNode);

            mapsNode.Nodes.Add(fileNode);
            mapsNode.Nodes.Add(addNode);

        }

        private void ProjectOnNpcDeleted(object sender, FileEventArgs args)
        {
            var nodeToDelete = treeProject.FindNode($"Default\\Game Data\\Npcs\\{args.File.Name}");

            this.FileRemoved?.Invoke(this, new FileEventArgs(args.File));

            nodeToDelete?.ParentNode.Nodes.Remove(nodeToDelete);
        }

        private void ProjectOnNpcAdded(object sender, FileEventArgs args)
        {
            DarkTreeNode fileNode = new DarkTreeNode(args.File.Name)
            {
                Icon = Icons.document_16xLG,
                Tag = args.File
            };

            var npcsNode = treeProject.FindNode($"Default\\Game Data\\Npcs");

            var addNode = treeProject.FindNode($"Default\\Game Data\\Npcs\\Add NPC");
            npcsNode.Nodes.Remove(addNode);

            npcsNode.Nodes.Add(fileNode);
            npcsNode.Nodes.Add(addNode);
        }

        private void ProjectOnItemDeleted(object sender, FileEventArgs args)
        {
            var nodeToDelete = treeProject.FindNode($"Default\\Game Data\\Items\\{args.File.Name}");

            this.FileRemoved?.Invoke(this, new FileEventArgs(args.File));

            nodeToDelete?.ParentNode.Nodes.Remove(nodeToDelete);
        }

        private void ProjectOnItemAdded(object sender, FileEventArgs args)
        {
            DarkTreeNode fileNode = new DarkTreeNode(args.File.Name)
            {
                Icon = Icons.document_16xLG,
                Tag = args.File
            };

            var itemsNode = treeProject.FindNode($"Default\\Game Data\\Items");

            var addNode = treeProject.FindNode($"Default\\Game Data\\Items\\Add Item");
            itemsNode.Nodes.Remove(addNode);

            itemsNode.Nodes.Add(fileNode);
            itemsNode.Nodes.Add(addNode);
        }

        private void TryAppendDirectoryNode(DirectoryInfo directory)
        {
            // Make sure a node for this directory does not already exist.
            foreach (var childNode in this.treeProject.SelectedNodes[0].Nodes)
            {
                if (childNode.Tag is DirectoryInfo)
                {
                    if (((DirectoryInfo)childNode.Tag).FullName == directory.FullName)
                    {
                        return;
                    }
                }
            }

            var node = new DarkTreeNode(directory.Name)
            {
                Icon = Icons.folder_closed,
                ExpandedIcon = Icons.folder_open,
                Expanded = true
            };

            this.treeProject.SelectedNodes[0].Nodes.Add(node);
            this.treeProject.SelectNode(node);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.treeProject.SelectedNodes.Count > 0)
            {
                if (this.treeProject.SelectedNodes[0].Tag is FileInfo info)
                {
                    switch (info.Extension)
                    {
                        case EngineConstants.ITEM_FILE_EXT:
                            _project.RemoveItem(info.FullName);
                            break;

                        case EngineConstants.NPC_FILE_EXT:
                            _project.RemoveNPC(info.FullName);
                            break;

                        case EngineConstants.ANIM_FILE_EXT:
                            _project.RemoveAnimations(info.FullName);
                            break;

                        case EngineConstants.MAP_FILE_EXT:
                            _project.RemoveMap(info.FullName);
                            break;

                        case EngineConstants.SCRIPT_FILE_EXT:
                            _project.RemoveScript(info.FullName);
                            break;
                    }
                }
            }
        }

        public event EventHandler<FileEventArgs> FileSelected;

        public event EventHandler<FileEventArgs> FileCreated;

        public event EventHandler<GameFileChangedEventArgs> FileChanged;

        public event EventHandler<FileEventArgs> FileRemoved;

        
        public string GetNextAvailableFilename(string fullFileName)
        {
            if (!System.IO.File.Exists(fullFileName)) return fullFileName;

            string alternateFilename;
            int fileNameIndex = 1;
            do
            {
                fileNameIndex += 1;
                string plainName = System.IO.Path.GetFileNameWithoutExtension(fullFileName);
                string extension = System.IO.Path.GetExtension(fullFileName);
                alternateFilename = string.Format("{0}{1}{2}", plainName, fileNameIndex, extension);
            } while (System.IO.File.Exists(alternateFilename));

            return alternateFilename;
        }
    }
}
