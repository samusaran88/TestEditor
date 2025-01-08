using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : UnitySingleton<UndoRedoManager>
{
    private Stack<Command> _undoStack = new Stack<Command>();
    private Stack<Command> _redoStack = new Stack<Command>();
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Undo(); 
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                Redo();  
            }
        }
    }
    public void ExecuteCommand(Command command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();  // Clear redo stack when a new action is made
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            Command command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }
    }

    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            Command command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }
    }
}
public abstract class Command
{
    public abstract void Execute();
    public abstract void Undo();
}
public class ActionCommand : Command
{
    private readonly System.Action _executeAction;
    private readonly System.Action _undoAction;

    public ActionCommand(System.Action executeAction, System.Action undoAction)
    {
        _executeAction = executeAction;
        _undoAction = undoAction;
    }

    public override void Execute()
    {
        _executeAction?.Invoke();
    }

    public override void Undo()
    {
        _undoAction?.Invoke();
    }
}