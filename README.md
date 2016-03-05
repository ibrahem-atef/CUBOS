# CUBOS
Black Oil Reservoir Simulator built using the C# programming language

The "data" folder contains some input datafiles for all the problems in the both books on reservoir simulation that are co-written by Abu El Qasim

I am currently more problems from the books

I am a detailed oriented person, so i might be a bit slow at crafting quality slides in PPT versus coding or writing in general.

The code includes dedicated portios "solvers" for different simulation problems "incompressible, slightly-compressible and compressible" in this particular order

The 3 are backwards compatible, which means that a slightly-compressible solver will be perfectly able to handle an incmopressible fluid problem y assuming 0 compressibilities. However, the respective codes were kept for improving readability. It's much easier to follow the code of a single-phase incompressible fluid than a multi-phase solver.

Sometimes, the most effiecient-code is very not suitable for reading and further modifications. Also, improving code readability means that some decisions have to be made even though they go against performance.


The science is simple and extensively discussed in literature "in this case for black oil simulation". The implementation is also straight-forward. However, the most daunting task is about writing code that is flexible enough for improvement and modifications and simple to read and follow along.


I have re-written the whole thing from scratch five times now and in every single time i come up with better decisions about how things should be organized.

Some parts of the code are too small to implement in a separate method. This will only add layers of unnecessary complexity to the reader and potential pitfalls in performance.

The code is much more organized and easier to investigate if opened in VisualStudio.

Using an SVN "Software Versioning Numbering" system such as github allows for better code distribution. It also provides an easier way to keep track of code changes.
