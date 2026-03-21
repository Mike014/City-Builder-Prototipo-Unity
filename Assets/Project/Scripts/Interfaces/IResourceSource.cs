public interface IResourceSource 
{
    bool TryProvideResource(out int amount);
}

/*
Farm → IResourceSource per il cibo
Factory → IResourceSource per i posti di lavoro
*/