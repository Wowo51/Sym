# Sym Docs

## SymApp GUI

It is recommended that you explore the GUI first. It is  a fairly thin layer on top of the Sym library and you will get a chance to explore Sym's functionality before writing any code that calls the library directly. You should probably start with the Transform section as transforms are central to Sym's operation.


### Transform
Download the code, compile, and start the SymApp GUI. The large text box is the main work space, you will enter the equations that you want to transform there. Look towards the right of the app and you will see some transforms. Explore the tabs there and examine some of the transforms. Understanding what a Sym transform does is crucial to using the Sym library. Each transform is a mathematical law. Only transforms which preserve the equality of the expression should be entered in the transform tabs. Towards the bottom right of the app you will see an 'Active Transform' box. Double click on a transform in a transform tab and you should see it appear in the 'Active Transform' box. Select the a+b~b+a transform from the Algebra, Commutative tab. Enter the following expression into the main work space, x+y\*z. Leave the cursor on the line with your expression and click the Transform button in the toolbar. With any luck Sym will transform your expression. Avoid the transforms with capital C's and V's in them for now, those are special characters to Sym that will be described later on. Keep on entering random expressions in the main work space that you think will transform with a given transform and explore the functionality until you are comfortable with what a transform does. A capital C forces Sym to ensure that a number appropriately exists instead of a general expression. A capital V forces Sym to ensure that a variable appropriately exists instead of a general expression.


### Evaluate
Enter an expression that contains only numbers into the main work space. Leave the cursor on the expression you entered and click the Evaluate button. Your expression should reduce.


### Simplify
Enter a simple expression that could be reduced to simpler form in the work space. Leave the cursor on your expression and click Simplify. Sym's solver will use all of the algebraic transforms on your expression and will attempt to shorten your expression. Note that combinatorial brute force searches can be slow for large expressions. I will be actively improving Sym's solvers as time goes on.


### Substitute
This function operates on the selected expression in the work space and prompts you for a transform. The transform will substitute in the value on the right of the transform at all appropriate points in the expression. You can use the V symbol to select variables. For example Vx~5 will replace all x variables in your expression with the number 5.


### Isolate
This function operates on the selected expression in the work space and prompts for a variable to isolate. It uses Sym's solver and algebra transforms to attempt to isolate your variable.


### CopyLine
This function copies the selected expression in the work space.


### Derivative
Takes the derivative of the active expression.


### Partial Derivative
Takes the partial derivative of the active expression.


### Transform Branches
Applies the active transform to all relevant points of the active expression.

### Stop Solving
Stops the solver.

### Menu
The menu functions should be self explanatory for the most part.

## Sym Library

The Node class can hold a mathematical expression in tree form. It is the fundamental unit of computation in Sym. Before you can use the Transform and Evaluation classes you have to call Node.Parse and pass in a string containing a mathematical expression to convert the string to a Node tree. You can call Node.Join to convert back to a string.

The Transform class is also elemental to the library. You can transform with the TransformNode function. See the code in the GUI for an example of converting a string containing a mathematical expression into Node form and calling the Transform class.

The TransformBranchFunctions class is built on top of the Transform class. The TransformBranchFunctions allow you to transform at locations in the expression other than the root of the expression.

The Solver class is built on top of TransformBranchFunctions. It contains a general purpose solver that can be customized for different situations

The Solvers class is built on top of the Solver class. Solvers contains several specific solvers such as Simplify and Isolate.

The Goals classes allow you to write custom solvers on top of the Solver class.