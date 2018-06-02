namespace flickr.uploader.application
{
    public interface ICommandHandler<in T>
    {
        void Handle(T command);
    }

    public interface ICommandHandler<in TCommand, out TResult>
    {
        TResult Handle(TCommand command);
    }
}