## Example CPLEX program using the Concert API for .Net

CPLEX is a convex optimization toolkit for solving a wide variety of problems. 

Convex optimization is a more general approach to solving mathematical optimization problems that encompasses classic linear 
programming, systems of equations, and least-squares techniques. A very good reference on the subject can be found 
in [Convex Optimization – Boyd and Vandenberghe](https://web.stanford.edu/~boyd/cvxbook/) for those who
are interested in the subject. It contains a good balance of theory and practical applications,
along with algorithmic descriptions.

The goal of this example program is to demonstrate how the API for CPLEX is used to setup 
an optimization problem and solve it. CPLEX refers to this API as "Concert." CPLEX has its own language (OPL) for expressing
problems and allowing to them be run in the CPLEX environment, along with a Javascript-like
procedural language (ILOG) to manage pre/post data processing and control flow However, for cases
where a model must be integrated into other applications, the CPLEX API is available for,
C/C++, Python, and .Net shops to take advantage of.

The CPLEX package comes with a lot of example code for OPL and extensive application examples, 
80% of which are written in native OPL/ILOG. The one financial example for portfolio optimization
is in OPL. I was interested in porting this to the API as an exercise to get acquainted with the API and 
optimization studio tools.




