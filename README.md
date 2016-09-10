# ghostline

GhostLine allows you to quickly set up nodes that create data and feed off of other data.

GhostLine uses the .NET BlockingCollection class, but adds a number of additional capabilities including:

1) Adding lambda delegates to nodes so you don't have to create subclasses to do simple processing  
2) Producers can distribute to multiple consumers  
3) Built in cycle checking to make sure consumers are not their own producers (and vice versa)  
4) Control over parallelism at multiple levels (using TPL)  

Simple Example:

    var origin = new GhostLine<int>();

    var doubler = new GhostLine<int>();
    origin.AddConsumer(doubler);

    origin.AddWorkUnit(5);
    origin.AddWorkUnit(6);
    origin.AddWorkUnit(7);

    doubler.Process = w =>
    {
        int x = w * 2;
        outputList.Add(x);
        return x;
    };
    
    origin.Begin();
    origin.CompleteAdding();


