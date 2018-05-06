using System;
using System.Linq;
using StructureMap;

namespace flickr.uploader
{
    public class CommandDispatcher
    {
        private readonly IContainer _container;

        public CommandDispatcher(IContainer container)
        {
            _container = container;
        }

        public void Dispatch<T>(T command)
        {
            var handlers = _container.GetAllInstances<ICommandHandler<T>>();
            if (handlers.Any() == false) {
                throw new Exception("No handlers found");
            }
            foreach (var handler in handlers) {
                handler.Handle(command);
            }
        }

        public TResult Dispatch<TCommand, TResult>(TCommand command)
        {
            var handler = _container.GetInstance<ICommandHandler<TCommand, TResult>>();
            if (handler == null) {
                throw new Exception("No handlers found");
            }
            return handler.Handle(command);
        }
    }
}