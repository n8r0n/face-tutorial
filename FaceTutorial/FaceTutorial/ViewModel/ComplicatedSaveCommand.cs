using System;
using System.Windows.Input;

namespace FaceTutorial.ViewModel
{
    class ComplicatedSaveCommand : ICommand
    {
        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            // TODO: add implementation here
        }
    }
}
