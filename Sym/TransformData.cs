//Copyright Warren Harding 2020. Released under the MIT.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sym
{
    public class TransformData
    {
        public static List<Transform> AllAlgebraTransformsAsTransforms(List<Operator> operators)
        {
            return AllAlgebraTransformsAsStrings().Select(x => Transform.StringToTransform(x, operators)).ToList();
        }

        public static List<Transform> DerivativeTransformsAsTransforms(List<Operator> operators)
        {
            return Derivatives().Select(x => Transform.StringToTransform(x, operators)).ToList();
        }

        public static List<Transform> PartialDerivativeTransformsAsTransforms(List<Operator> operators)
        {
            return PartialDerivatives().Select(x => Transform.StringToTransform(x, operators)).ToList();
        }

        public static List<string> AllAlgebraTransformsAsStrings()
        {
            List<string> lOut = new List<string>();
            lOut.AddRange(TransformData.Commutative());
            lOut.AddRange(TransformData.Distributive());
            lOut.AddRange(TransformData.Elementary());
            lOut.AddRange(TransformData.SimplifyingAlgebraic());
            lOut.AddRange(TransformData.Exponential());
            lOut.AddRange(TransformData.Factoring());
            //lOut.AddRange(TransformLists.Trigonometric());
            //lOut.AddRange(TransformStrings.Derivatives());
            return lOut;
        }

        public static Dictionary<string, string> AllAlgebraAsNamedSets()
        {
            Dictionary<string, string> lOut = new Dictionary<string, string>();
            lOut.Add("Commutative", CommutativeString());
            lOut.Add("Distributive", DistributiveString());
            lOut.Add("Elementary", ElementaryString());
            lOut.Add("Simplifying", SimplifyingAlgebraicString());
            lOut.Add("Exponential", ExponentialString());
            lOut.Add("Trigonometric", TrigonometricString());
            lOut.Add("Factoring", FactoringString());
            return lOut;
        }

        public static List<string> Commutative()
        {
            List<string> lOut = new List<string>();
            lOut.Add("a+b~b+a");
            lOut.Add("a+b+c~a+c+b");
            lOut.Add("a-b~-b+a");
            lOut.Add("a*b~b*a");
            lOut.Add("a*b*c~a*c*b");
            lOut.Add("a=b~b=a");
            return lOut;
        }

        public static List<string> Distributive()
        {
            List<string> lOut = new List<string>();
            lOut.Add("a*(b+c)~a*b+a*c");
            lOut.Add("a*(b-c)~a*b-a*c");
            lOut.Add("a*c+b*c~(a+b)*c");
            lOut.Add("a*c-b*c~(a-b)*c");
            lOut.Add("a/c+b/c~(a+b)/c");
            lOut.Add("a/c-b/c~(a-b)/c");
            lOut.Add("(a+b)/c~a/c+b/c");
            lOut.Add("(a-b)/c~a/c-b/c");
            lOut.Add("a*b+a~a*(b+1)");
            lOut.Add("a*b-a~a*(b-1)");

            return lOut;
        }

        public static List<string> Elementary()
        {
            List<string> lOut = new List<string>();
            lOut.Add("a*b=c~a=c/b");
            lOut.Add("a+b=c~a=c-b");
            lOut.Add("a-b=c~a=c+b");
            lOut.Add("a/b=c~a=c*b");

            lOut.Add("a=b*c~a/c=b");
            lOut.Add("a=b+c~a-c=b");
            lOut.Add("a=b-c~a+c=b");
            lOut.Add("a=b/c~a*c=b");

            lOut.Add("a=b*c~a/c=b");
            lOut.Add("a=b+c~a-c=b");
            lOut.Add("a=b-c~a+c=b");
            lOut.Add("a=-b+c~a-c=-b");
            lOut.Add("a=b/c~a*c=b");

            lOut.Add("a*b=c~b=c/a");
            lOut.Add("a+b=c~b=c-a");
            lOut.Add("a-b=c~b=a-c");
            lOut.Add("a/b=c~b=a/c");

            lOut.Add("-a=b~a=-b");
            lOut.Add("a*b=a*c~b=c");
            lOut.Add("a+b=a+c~b=c");
            lOut.Add("b-a=c-a~b=c");
            lOut.Add("b/a=c/a~b=c");
            lOut.Add("a-b=a-c~b=c");
            lOut.Add("a/b=a/c~b=c");
            lOut.Add("a/b=c~b=a/c");
            lOut.Add("a+b+c~(a+b)+c");
            lOut.Add("a*b*c~(a*b)*c");
            lOut.Add("a-b-c~(a-b)-c");
            lOut.Add("a/b/c~(a/b)/c");
            lOut.Add("a-b-c~a-c-b");
            lOut.Add("a/b/c~a/c/b");
            lOut.Add("a*b*c~a*c*b");
            lOut.Add("a+b+c~a+c+b");
            lOut.Add("a*b/c~(a/c)*b");
            lOut.Add("a+b*a~(b+1)*a");
            lOut.Add("a-b*a~(1-b)*a");
            lOut.Add("-(-a-b)~a+b");
            lOut.Add("a-b-c~a-c-b");
            lOut.Add("a-b-c=d~a-b=d+c");
            lOut.Add("a=b-c~a-b=-c");
            lOut.Add("a-(b-c)~a-b+c");
            lOut.Add("a+b+c=d~a+b=d-c");
            lOut.Add("a+b+c+d~(a+b)+(c+d)");
            //lOut.AddRange(Isolate());
            return lOut;
        }

        //public static List<string> Isolate()
        //{
        //    List<string> lOut = new List<string>();
        //    lOut.Add("a*b=c~a=c/b");
        //    lOut.Add("a*b=c~b=c/a");
        //    lOut.Add("a+b=c~a=c-b)");
        //    lOut.Add("a+b=c~b=c-a)");
        //    lOut.Add("a-b=c~a=c+b");
        //    lOut.Add("a-b=c~b=a-c");
        //    lOut.Add("a/b=c~a=c*b");
        //    lOut.Add("a/b=c~b=a/c");
        //    lOut.Add("-a=b~a=-b");
        //    lOut.Add("(a)=b~a=b");
        //    lOut.Add("Pow(a,b)=c~b=Log(c,a)");
        //    //lOut.Add("Sin(fx)=fy~fx=Asin(fy)");
        //    //lOut.Add("Cos(fx)=fy~fx=Acos(fy)");
        //    //lOut.Add("Tan(fx)=fy~fx=Atan(fy)");
        //    //lOut.Add("Asin(fx)=fy~fx=Sin(fy)");
        //    //lOut.Add("Acos(fx)=fy~fx=Cos(fy)");
        //    //lOut.Add("Atan(fx)=fy~fx=Tan(fy)");
        //    return lOut;
        //}

        public static List<string> SimplifyingAlgebraic()
        {
            List<string> lOut = new List<string>();
            lOut.Add("a-a~0");
            lOut.Add("a/a~1");
            lOut.Add("a+0~a");
            lOut.Add("a-0~a");
            lOut.Add("a*1~a");
            lOut.Add("a/1~a");
            lOut.Add("a*0~0");
            lOut.Add("0+a~a");
            lOut.Add("1*a~a");
            lOut.Add("0*a~0");
            lOut.Add("0/a~0");
            lOut.Add("(a+b)+c~a+b+c");
            lOut.Add("(a+b)-c~a+b-c");
            lOut.Add("(a-b)+c~a-b+c");
            lOut.Add("(a-b)-c~a-b-c");
            lOut.Add("(a*b)+c~a*b+c");
            lOut.Add("(a/b)+c~a/b+c");
            lOut.Add("a+(b+c)~a+b+c");
            lOut.Add("a+(b-c)~a+b-c");
            lOut.Add("a+(b*c)~a+b*c");
            lOut.Add("a+(b/c)~a+b/c");
            lOut.Add("a-(b*c)~a-b*c");
            lOut.Add("a-(b/c)~a-b/c");
            lOut.Add("a*(b*c)~a*b*c");
            lOut.Add("a*(b/c)~a*b/c");
            //lOut.Add("((a))~(a)");
            lOut.Add("(a)~a");
            lOut.Add("-a=-b~a=b");
            lOut.Add("-a=-b~a=-b*-1");
            //lOut.Add("a+(b*c)~a+b*c");
            //lOut.Add("a-(b*c)~a-b*c");
            //lOut.Add("a+(b/c)~a+b/c");
            //lOut.Add("a-(b/c)~a-b/c");
            lOut.Add("Pow(a,1)~a");
            lOut.Add("Pow(a,0)~1");
            //lOut.Add("(-V)*a~-V*a");
            //lOut.Add("(-V)+a~-V+a");
            //lOut.Add("(-V)-a~-V-a");
            //lOut.Add("(-V)/a~-V/a");
            //lOut.Add("(-C)*a~-C*a");
            //lOut.Add("(-C)+a~-C+a");
            //lOut.Add("(-C)-a~-C-a");
            //lOut.Add("(-C)/a~-C/a");
            //lOut.Add("x-(-V)~x+V");
            //lOut.Add("x+(-V)~x-V");
            //lOut.Add("x-(-C)~x+C");
            //lOut.Add("x+(-C)~x-C");
            //lOut.Add("(C)~C");
            //lOut.Add("(-C)~-C");
            //lOut.Add("(V)~V");
            //lOut.Add("(-V)~-V");
            lOut.Add("C1*C2*a~(C1*C2)*a");
            lOut.Add("C1/C2/a~(C1/C2)/a");
            lOut.Add("C1+C2+a~(C1+C2)+a");
            lOut.Add("C1-C2-a~(C1-C2)-a");
            //lOut.Add("a--b~a+b");
            //lOut.Add("a+-b~a-b");

            return lOut;
        }

        public static List<string> Exponential()
        {
            List<string> lOut = new List<string>();
            lOut.Add("Pow((Pow(a,b)),c)~Pow(a,b*c)");
            lOut.Add("Pow(a,1)~a");
            lOut.Add("Pow(a,0)~1");
            lOut.Add("Pow(a,(-1))~1/a");
            lOut.Add("Pow(a,b)*Pow(a,c)~Pow(a,(b+c))");
            lOut.Add("Pow(a,b)/Pow(a,c)~Pow(a,(b-c))");
            lOut.Add("Pow(a*b,c)~Pow(a,c)*Pow(b,c)");
            lOut.Add("Pow(a/b,c)~Pow(a,c)/Pow(b,c)");
            lOut.Add("Pow(a,-b)~1/Pow(a,b)");
            lOut.Add("Pow(a+b,2)~Pow(a,2)+2*a*b+Pow(2,2)");
            lOut.Add("Pow(a-b,2)~Pow(a,2)-2*a*b+Pow(2,2)");
            lOut.Add("Pow(a,b)=c~b=Log(c,a)");
            lOut.Add("Log(a,b)=c~Pow(b,c)=a");
            lOut.Add("Pow(a,b)=c~a=Pow(c,1/b)");
            lOut.Add("Pow(Sqrt(a),2)~a");
            lOut.Add("Sqrt(Pow(a,2))~a");
            lOut.Add("Pow(a,(b))~Pow(a,b)");
            lOut.Add("Pow((a),b)~Pow(a,b)");
            lOut.Add("Log(a*b,c)~Log(a,c)+Log(b,c)");
            lOut.Add("Log(a/b,c)~Log(a,c)-Log(b,c)");
            lOut.Add("Log(Pow(a,b),c)~b*Log(a,c)");
            lOut.Add("Log(Pow(a,1/b),c)~Log(a,c)/b");
            lOut.Add("Log(a,a)~1");
            lOut.Add("Log(1,a)~0");
            lOut.Add("-a*a~-Pow(a,2)");
            lOut.Add("a*a~Pow(a,2)");
            //lOut = AddInequalities(lOut);
            return lOut;
        }

        public static List<string> Trigonometric()
        {
            List<string> lOut = new List<string>();
            lOut.Add("Sin(a)/Cos(a)~Tan(a)");
            lOut.Add("Sin(Asin(a))~a");
            lOut.Add("Cos(Acos(a))~a");
            lOut.Add("Tan(Atan(a))~a");
            lOut.Add("Pow(Sin(a),2)+Pow(Cos(a),2)~1");
            //lOut.Add("Sin(fx)=fy~fx=Asin(fy)");
            //lOut.Add("Cos(fx)=fy~fx=Acos(fy)");
            //lOut.Add("Tan(fx)=fy~fx=Atan(fy)");
            //lOut.Add("Asin(fx)=fy~fx=Sin(fy)");
            //lOut.Add("Acos(fx)=fy~fx=Cos(fy)");
            //lOut.Add("Atan(fx)=fy~fx=Tan(fy)");
            //lOut = AddInequalities(lOut);
            return lOut;
        }

        public static List<string> Factoring()
        {
            List<string> lOut = new List<string>();
            //lOut.Add("(a-b)*c=0~a=b");
            //lOut.Add("(a+b)*c=0~a=-b");
            lOut.Add("a*Pow(x,2)+b*x+c=0~x=(-b+Sqrt(Pow(b,2)-4*a*c))/(2*a)");
            lOut.Add("a*Pow(x,2)+b*x+c=0~x=(-b-Sqrt(Pow(b,2)-4*a*c))/(2*a)");
            return lOut;
        }

        public static string CommutativeString()
        {
            return string.Join(Environment.NewLine, Commutative());
        }

        public static string DistributiveString()
        {
            return string.Join(Environment.NewLine, Distributive());
        }

        public static string ElementaryString()
        {
            return string.Join(Environment.NewLine, Elementary());
        }

        public static string SimplifyingAlgebraicString()
        {
            return string.Join(Environment.NewLine, SimplifyingAlgebraic());
        }

        public static string ExponentialString()
        {
            return string.Join(Environment.NewLine, Exponential());
        }

        public static string TrigonometricString()
        {
            return string.Join(Environment.NewLine, Trigonometric());
        }

        public static string FactoringString()
        {
            return string.Join(Environment.NewLine, Factoring());
        }

        public static Dictionary<string, string> AllCalculusAsNamedSets()
        {
            Dictionary<string, string> lOut = new Dictionary<string, string>();
            lOut.Add("Derivative", DerivativesString());
            lOut.Add("Integral", IntegralsString());
            lOut.Add("Partial Derivative", PartialDerivativesString());
            return lOut;
        }

        public static List<string> Derivatives()
        {
            List<string> lOut = new List<string>();
            lOut.Add("d(C)/d(x)~0");
            lOut.Add("d(Pow(x,C))/d(x)~C*Pow(x,(C-1))");
            lOut.Add("d(u+v)/d(x)~d(u)/d(x)+d(v)/d(x)");
            lOut.Add("d(u-v)/d(x)~d(u)/d(x)-d(v)/d(x)");
            lOut.Add("d(C*u)/d(x)~C*d(u)/d(x)");
            lOut.Add("d(u*C)/d(x)~C*d(u)/d(x)");
            lOut.Add("d(C*x)/d(x)~C");
            lOut.Add("d(x*C)/d(x)~C");
            lOut.Add("d(u*v)/d(x)~u*d(v)/d(x)+v*d(u)/d(x)");
            lOut.Add("d(u/v)/d(x)~(v*d(u)/d(x)-u*d(v)/d(x))/Pow(v,2)");
            //lOut.Add("d(Pow(u,C))/d(x)~C*Pow(u,(C-1))*d(u)/d(x)");
            //lOut.Add("d(y)/d(x)~d(y)/d(u)*d(u)/d(x)");
            lOut.Add("d(Sin(x))/d(x)~Cos(x)");
            lOut.Add("d(Cos(x))/d(x)~-Sin(x)");
            lOut.Add("d(Tan(x))/d(x)~Sec(Sec(x))");
            lOut.Add("d(Sec(x))/d(x)~Tan(x)*Sec(x)");
            lOut.Add("d(Cot(x))/d(x)~-Csc(Csc(x))");
            lOut.Add("d(Csc(x))/d(x)~-Cot(x)*Csc(x)");
            lOut.Add("d(Log(x))/d(x)~1/x");
            lOut.Add("d(Log(x,b))/d(x)~1/(x*Log(b))");

            lOut.Add("x*d(C)/d(x)~0");
            lOut.Add("y*d(x)/d(x)~y");
            lOut.Add("C*d(x)/d(x)~C");
            lOut.Add("u*d(C*x)/d(x)~u*C");
            return lOut;
        }

        public static string PartialDerivativesString()
        {
            return string.Join(Environment.NewLine, PartialDerivatives());
        }

        public static List<string> PartialDerivatives()
        {
            List<string> lOut = new List<string>();
            lOut.Add("p(C)/p(V)~0");
            lOut.Add("p(V2)/p(V1)~0");
            lOut.Add("p(Pow(V,C))/p(V)~C*Pow(V,(C-1))");
            lOut.Add("p(Pow(V1,V2))/p(V1)~V2*Pow(V1,(V2-1))");
            lOut.Add("p(f1+f2)/p(V)~p(f1)/p(V)+p(f2)/p(V)");
            lOut.Add("p(f1-f2)/p(V)~p(f1)/p(V)-p(f2)/p(V)");
            lOut.Add("p(C*f1)/p(V)~C*p(f1)/p(V)");
            lOut.Add("p(f1*C)/p(V)~C*p(f1)/p(V)");
            lOut.Add("p(C*V)/p(V)~C");
            lOut.Add("p(V*C)/p(V)~C");
            lOut.Add("p(V2*V1)/p(V1)~V2");
            lOut.Add("p(V1*V2)/p(V1)~V2");
            lOut.Add("p(f1*f2)/p(V)~f1*p(f2)/p(V)+f2*p(f1)/p(V)");
            lOut.Add("d(f1/f2)/d(V)~(f2*d(f2)/d(V)-f1*d(f2)/d(V))/Pow(f2,2)");
            //lOut.Add("p(Pow(x,V))/p(x)~V*Pow(x,(V-1))");
            //lOut.Add("p(V*u)/p(x)~V*p(u)/p(x)");
            //lOut.Add("p(u*V)/p(x)~V*p(u)/p(x)");
            //lOut.Add("p(V*x)/p(x)~V");
            //lOut.Add("p(x*V)/p(x)~V");
            return lOut;
        }

        public static List<string> Integrals()
        {
            List<string> lOut = new List<string>();
            lOut.Add("i(Pow(x,k))*d(x)~Pow(x,k+1)/(k+1)+C");
            lOut.Add("i(a*f)*d(x)~a*i(f)*d(x)");
            lOut.Add("i(f+g)*d(x)~i(f)*d(x)+i(g)*d(x)");
            lOut.Add("i(f-g)*d(x)~i(f)*d(x)-i(g)*d(x)");
            return lOut;
        }

        public static string DerivativesString()
        {
            return string.Join(Environment.NewLine, Derivatives());
        }

        public static string IntegralsString()
        {
            return string.Join(Environment.NewLine, Integrals());
        }

        public static Dictionary<string, string> AllVectorAsNamedSets()
        {
            Dictionary<string, string> lOut = new Dictionary<string, string>();
            lOut.Add("Vector 2D", Vector2String());
            lOut.Add("Vector 3D", Vector3String());
            return lOut;
        }

        public static List<string> Vector3()
        {
            List<string> lOut = new List<string>();
            lOut.Add("(x1,y1,z1)+(x2,y2,z2)~x1+x2,y1+y2,z1+z2");
            lOut.Add("(x1,y1,z1)-(x2,y2,z2)~x1-x2,y1-y2,z1-z2");
            lOut.Add("k*(x1,y1,z1)~k*x1,k*y1,k*z1");
            lOut.Add("(x,y,z)*a~a*x,a*y,a*z");
            lOut.Add("(x,y,z)/(a)~x/a,y/a,z/a");
            lOut.Add("Length(x1,y1,z1)~Sqrt(Pow(x1,2)+Pow(y1,2)+Pow(z1,2))");
            lOut.Add("Distance((x1,y1,z1),(x2,y2,z2))~Sqrt(Pow(x1-x2,2)+Pow(y1-y2,2)+Pow(z1-z2,2))");
            lOut.Add("Dot((x1,y1,z1),(x2,y2,z2))~x1*x2+y1*y2+z1*z2");
            lOut.Add("Cross((x1,y1,z1),(x2,y2,z2))~y1*z2-z1*y2,x1*z2-z1*x2,x1*y2-y1*x2");
            lOut.Add("Grad(f)~p(f)/p(x),p(f)/p(y),p(f)/p(z)");
            lOut.Add("Div(fx,fy,fz)~p(fx)/p(x)+p(fy)/p(y)+p(fz)/p(z)");
            lOut.Add("Curl(fx,fy,fz)~p(fz)/p(y)-p(fy)/p(z),p(fx)/p(z)-p(fz)/p(x),p(fy)/p(x)-p(fx)/p(y)");
            return lOut;
        }

        public static List<string> Vector2()
        {
            List<string> lOut = new List<string>();
            lOut.Add("(x1,y1)+(x2,y2)~x1+x2,y1+y2");
            lOut.Add("(x1,y1)-v(x2,y2)~x1-x2,y1-y2");
            lOut.Add("k*(x,y)~k*x,k*y");
            lOut.Add("(x,y)*k~k*x,k*y");
            lOut.Add("d(x,y)/d(a)~d(x)/d(a),d(y)/d(a)");
            lOut.Add("Length(x,y)~Sqrt(Pow(x,2)+Pow(y,2))");
            lOut.Add("Distance((x1,y1),(x2,y2))~Sqrt(Pow(x1-x2,2)+Pow(y1-y2,2))");
            lOut.Add("Dot((x1,y1),(x2,y2))~x1*x2+y1*y2");
            return lOut;
        }

        public static string Vector3String()
        {
            return string.Join(Environment.NewLine, Vector3());
        }

        public static string Vector2String()
        {
            return string.Join(Environment.NewLine, Vector2());
        }

        public static Dictionary<string, string> AllLogicAsNamedSets()
        {
            Dictionary<string, string> lOut = new Dictionary<string, string>();
            lOut.Add("Logic 1", Logic1String());
            lOut.Add("Logic 2", Logic2String());
            //lOut.Add("Logic 3", TransformTexts.Logic3());
            return lOut;
        }

        public static List<string> Logic1()
        {
            List<string> lOut = new List<string>();
            lOut.Add("A=B&A=true~B=true");//modusponens
            lOut.Add("A=B&A=false~B=false");
            lOut.Add("A=B&B=true~A=true");
            lOut.Add("A=B&B=false~A=false");//modustollens
            lOut.Add("A!=B&A=true~B=false");
            lOut.Add("A!=B&A=false~B=true");
            lOut.Add("A!=B&B=true~A=false");
            lOut.Add("A!=B&B=false~A=true");
            //lOut.Add("A=B&A=true~B=true");//biconditionalelimination,materialequivalence1
            //lOut.Add("A=B&B=true~A=true");
            //lOut.Add("A=B&A=false~B=false");
            //lOut.Add("A=B&B=false~B=false");
            lOut.Add("A=B&(A|B)=true~A&B=true");
            lOut.Add("A=B&(!A|!B)=true~!A&!B=true");
            lOut.Add("A=true&B=true~(A&B)=true");//conjunctionintroduction
            lOut.Add("(A&B)=true~A=true");//simplification
            lOut.Add("(A&B)=true~B=true");
            lOut.Add("(A|B)=false~A=false");
            lOut.Add("(A|B)=false~B=false");
            //lOut.Add("(A|B)=true&A=false~B=true");
            //lOut.Add("(A|B)=true&B=false~A=true");


            //disjunctionintroduction
            lOut.Add("if(A=true){B=true;}if(A=false){B=true;}~B=true");//disjunctionelimination
            lOut.Add("(A^B)=true&A=false~B=true");//xordisjunctivesyllogism
            lOut.Add("(A^B)=true&B=false~A=true");
            lOut.Add("(A^B)=true&A=true~B=false");
            lOut.Add("(A^B)=true&B=true~A=false");
            lOut.Add("A=B&B=C~A=C");//hypotheticalsyllogism,chainrule

            lOut.Add("(A=B)&(C=D)&(A|C)=true~(B|D)=true");//constructivedilemma
            lOut.Add("(A=B)&(C=D)&(A=false|B=false)~(B|D)=false");//destructivedilemma
            //absorption

            lOut.Add("A=B&A=C&A=true~(B&C)=true");//composition

            lOut.Add("A&B~B&A");//commutation1
            lOut.Add("A&B&C~A&C&B");
            lOut.Add("A|B~B|A");//commutation2
            lOut.Add("A|B|C~A|C|B");
            lOut.Add("A=B~B=A");//commutation3

            lOut.Add("A|(B|C)~(A|B)|C");//association1
            lOut.Add("A&(B&C)~(A&B)&C");//association2
            lOut.Add("A&(B|C)~(A&B)|(A&C)");//distribution1
            lOut.Add("A|(B&C)~(A|B)&(A|C)");//distribution2

            lOut.Add("!A=!B~A=B");//transposition
            lOut.Add("A=B~!A=!B");

            lOut.Add("A=B~(!A|B)=true");//materialimplication

            lOut.Add("A=B~(A=true&B=true)^(A=false&B=false)");//materialequivalence2
            lOut.Add("A=B~(A|!B)=true&(!A|B)=true");//materialequivalence3



            lOut.Add("!A=true~A=false");
            lOut.Add("!A=false~A=true");

            lOut.Add("!(A&B)~!A|!B");
            lOut.Add("!(A|B)~!A&!B");


            lOut.Add("A|!A~true");//lawofexcludedmiddle
            lOut.Add("A&!A~false");//lawofnon-contradiction


            //lOut.Add("if (A = true) { B = true; } if (B = true) { A = true; } ~ A = B"); //biconditional introduction
            //lOut.Add("if ((A & B) =true) { C = true; } ~ if (A = true) { if (B = true ) { C = true; }}"); //exportation
            //lOut.Add("if (A = true) { if (B = true ) { C = true; }} ~ if ((A & B) =true) { C = true; }"); //importation
            return lOut;
        }

        public static List<string> Logic2()
        {
            List<string> lOut = new List<string>();
            lOut.Add("Union(A,B)~Union(B,A)");//commutative
            lOut.Add("Union(A,Union(B,C))~Union(Union(A,B),C)");
            lOut.Add("Intersection(A,B)~Intersection(B,A)");
            lOut.Add("Intersection(A,Intersection(B,C))~Intersection(Intersection(A,B),C)");
            lOut.Add("Union(Union(A,B),C)~Union(A,Union(B,C))");//associative
            lOut.Add("Intersection(Intersection(A,B),C)~Intersection(A,Intersection(B,C))");
            lOut.Add("Union(A,Intersection(B,C))~Intersection(Union(A,B),Union(A,C))");//distributive
            lOut.Add("Intersection(A,Union(B,C))~Union(Intersection(A,B),Intersection(A,C))");
            lOut.Add("Union(A,EmptySet)~A");//identity
            lOut.Add("Intersection(A,UniversalSet)~A");
            lOut.Add("Union(A,Complement(A))~UniversalSet");//complement
            lOut.Add("Intersection(A,Complement(A))~EmptySet");
            lOut.Add("Union(A,A)~A");//idempotent
            lOut.Add("Intersection(A,A)~A");
            lOut.Add("Union(A,EmptySet)~A");//domination
            lOut.Add("Intersection(A,UniversalSet)~A");
            lOut.Add("Union(A,Intersection(A,B))~A");//absorption
            lOut.Add("Intersection(A,Union(A,B))~A");
            lOut.Add("Complement(Union(A,B))~Intersection(Complement(A),Complement(B))");//DeMorgans
            lOut.Add("Complement(Intersection(A,B))~Union(Complement(A),Complement(B))");
            lOut.Add("Complement(Complement(A))~A");//involution
            lOut.Add("Complement(UniversalSet)~EmptySet");//complementsforemptyanduniversalsets
            lOut.Add("Complement(EmptySet)~UniversalSet");
            lOut.Add("Union(A,B)=UniversalSet&Intersection(A,B)=EmptySet~B=Complement(A)");//uniquenessofcomplements
            lOut.Add("SubSet(A,A)=true");//reflexivity
            lOut.Add("SubSet(A,B)&SubSet(B,A)~A=B");//antisymmetry
            lOut.Add("SubSet(A,B)&SubSet(B,C)~SubSet(A,C)");//transitivity
            lOut.Add("SubSet(A,S)&SubSet(EmptySet,SubSet(A,S))=true");//leastelement
            lOut.Add("SubSet(A,Union(A,B))=true");//joins
            lOut.Add("SubSet(A,B)&SubSet(A,C)~Intersection(A,SubSet(B,C))");
            lOut.Add("SubSet(Intersection(A,B),A");//meets
            lOut.Add("SubSet(C,A)&SubSet(C,B)~SubSet(C,Intersection(A,B))");
            lOut.Add("SubSet(A,B)&Intersection(A,B)~A");//proposition8
            lOut.Add("SubSet(A,B)&Union(A,B)~B");
            lOut.Add("Subtract(A,B)~EmptySet");
            lOut.Add("SubSet(Complement(B),Complement(A))=true");
            lOut.Add("Subtract(C,Intersection(A,B))~Union(Subtract(C,A),Subtract(C,B))");//proposition9
            lOut.Add("Subtract(C,Union(A,B))~Intersection(Subtract(C,A),Subtract(C,B))");
            lOut.Add("Subtract(C,Subtract(A,B))~Union(Intersection(A,C),Subtract(C,B))");
            lOut.Add("Intersection(Subtract(B,A),C)~Subtract(Intersection(B,C),A)");
            lOut.Add("Intersection(Subtract(B,A),C)~Intersection(B,Subtract(C,A))");
            lOut.Add("Union(Subtract(B,A),C)~Subtract(Union(B,C),Subtract(A,C))");
            lOut.Add("Subtract(A,A)~EmptySet");
            lOut.Add("Subtract(EmptySet,A)~EmptySet");
            lOut.Add("Subtract(A,EmptySet)~A");
            lOut.Add("Subtract(B,A)~Intersection(Complement(A),B)");
            lOut.Add("Complement(Subtract(B,A))~Union(A,Complement(B))");
            lOut.Add("Subtract(UniversalSet,A)~Complement(A)");
            lOut.Add("Subtract(A,UniversalSet)~EmptySet");
            return lOut;
        }


        public static List<string> Logic3()
        {
            List<string> lOut = new List<string>();
            lOut.Add("if (A = true) { B = true; } if (B = true) { C = true; } ~ if (A = true) { C = true; }");
            lOut.Add("if (A = true) { B = false; } if (B = false) { C = true; } ~ if (A = true) { C = true; }");
            lOut.Add("if (A = false) { B = true; } if (B = true) { C = true; } ~ if (A = false) { C = true; }");
            lOut.Add("if (A = false) { B = false; } if (B = false) { C = true; } ~ if (A = false) { C = true; }");
            lOut.Add("if (A = true) { B = true; } if (B = true) { C = false; } ~ if (A = true) { C = false; }");
            lOut.Add("if (A = true) { B = false; } if (B = false) { C = false; } ~ if (A = true) { C = false; }");
            lOut.Add("if (A = false) { B = true; } if (B = true) { C = false; } ~ if (A = false) { C = false; }");
            lOut.Add("if (A = false) { B = false; } if (B = false) { C = false; } ~ if (A = false) { C = false; }");

            lOut.Add("A | !A ~ true");
            lOut.Add("A & !A ~ false");
            return lOut;
        }

        public static string Logic1String()
        {
            return string.Join(Environment.NewLine, Logic1());
        }

        public static string Logic2String()
        {
            return string.Join(Environment.NewLine, Logic2());
        }

        public static string Logic3String()
        {
            return string.Join(Environment.NewLine, Logic3());
        }
    }
}
