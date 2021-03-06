using Xunit;

namespace DaedalusCompiler.Tests.SemanticErrors
{
    public class ReferenceResolvingVisitorTests : BaseSemanticErrorsTests
    {
        [Fact]
        public void TestAccessToAttributeOfArrayElement()
        {
            Code = @"
                func void myFunc() {
                    const int x[2] = {1, 2};
                    const int y = x[0].attr;
                    const int z = y[1].attr;
                    const int q = xxx[2].attr;
                    
                    var int a[2];
                    var int b;
                    b = a[0].attr;
                    b = b[1].attr;
                    b = xxx[2].attr;

                    var int new_a[2] = {a[0].attr, b[1].attr};
                    var int new_b = xxx[2].attr;
                }
            ";

            ExpectedCompilationOutput = @"
                test.d: In function 'myFunc':
                test.d:3:23: error: access to attribute of array element not supported
                    const int y = x[0].attr;
                                       ^
                test.d:4:18: error: cannot access array element because 'y' is not an array
                    const int z = y[1].attr;
                                  ^
                test.d:5:18: error: 'xxx' undeclared
                    const int q = xxx[2].attr;
                                  ^
                test.d:9:13: error: access to attribute of array element not supported
                    b = a[0].attr;
                             ^
                test.d:10:8: error: cannot access array element because 'b' is not an array
                    b = b[1].attr;
                        ^
                test.d:11:8: error: 'xxx' undeclared
                    b = xxx[2].attr;
                        ^
                test.d:13:29: error: access to attribute of array element not supported
                    var int new_a[2] = {a[0].attr, b[1].attr};
                                             ^
                test.d:13:35: error: cannot access array element because 'b' is not an array
                    var int new_a[2] = {a[0].attr, b[1].attr};
                                                   ^
                test.d:14:20: error: 'xxx' undeclared
                    var int new_b = xxx[2].attr;
                                    ^
                9 errors generated.
                ";

            AssertCompilationOutputMatch();
        }
        
        [Fact]
        public void TestUndeclaredIdentifierInsideFunctionDef()
        {
            Code = @"
                func void testFunc() {
                    x = 5;
                    x = a + b(c, d);
                };
            ";

            ExpectedCompilationOutput = @"
                test.d: In function 'testFunc':
                test.d:2:4: error: 'x' undeclared
                    x = 5;
                    ^
                test.d:3:4: error: 'x' undeclared
                    x = a + b(c, d);
                    ^
                test.d:3:8: error: 'a' undeclared
                    x = a + b(c, d);
                        ^
                test.d:3:12: error: 'b' undeclared
                    x = a + b(c, d);
                            ^
                test.d:3:14: error: 'c' undeclared
                    x = a + b(c, d);
                              ^
                test.d:3:17: error: 'd' undeclared
                    x = a + b(c, d);
                                 ^
                6 errors generated.
                ";

            AssertCompilationOutputMatch();
        }
        
        [Fact]
        public void TestUndeclaredIdentifierConstAssignment()
        {
            Code = @"
                const int a = b;
            ";

            ExpectedCompilationOutput = @"
                test.d:1:14: error: 'b' undeclared
                const int a = b;
                              ^
                1 error generated.
                ";

            AssertCompilationOutputMatch();
        }
        
        [Fact]
        public void TestAttributeNotFound()
        {
            Code = @"
                class NPC {
                    var int y;
                };

                instance self(NPC) {};
                var NPC victim;

                func void testFunc() {
                    self.x = 5;
                    self.y = 10;
                    other.y = 15;
                    victim.x = 20;
                    victim.y = 25;

                    var int a = self.x;
                    var int b = self.y;
                    var int c = other.y;
                    var int d = victim.x;
                    var int e = victim.y;
                    var int tab[5] = {self.x, self.y, other.y, victim.x, victim.y};
                };
            ";

            ExpectedCompilationOutput = @"
                test.d: In function 'testFunc':
                test.d:9:9: error: object 'self' of type 'NPC' has no member named 'x'
                    self.x = 5;
                         ^
                test.d:11:4: error: 'other' undeclared
                    other.y = 15;
                    ^
                test.d:12:11: error: object 'victim' of type 'NPC' has no member named 'x'
                    victim.x = 20;
                           ^
                test.d:15:21: error: object 'self' of type 'NPC' has no member named 'x'
                    var int a = self.x;
                                     ^
                test.d:17:16: error: 'other' undeclared
                    var int c = other.y;
                                ^
                test.d:18:23: error: object 'victim' of type 'NPC' has no member named 'x'
                    var int d = victim.x;
                                       ^
                test.d:20:27: error: object 'self' of type 'NPC' has no member named 'x'
                    var int tab[5] = {self.x, self.y, other.y, victim.x, victim.y};
                                           ^
                test.d:20:38: error: 'other' undeclared
                    var int tab[5] = {self.x, self.y, other.y, victim.x, victim.y};
                                                      ^
                test.d:20:54: error: object 'victim' of type 'NPC' has no member named 'x'
                    var int tab[5] = {self.x, self.y, other.y, victim.x, victim.y};
                                                                      ^
                9 errors generated.
                ";

            AssertCompilationOutputMatch();
        }
        
        
        [Fact]
        public void TestAttributeOfNonInstance()
        {
            Code = @"
                //! suppress: W5

                prototype Proto(X) {};

                func void testFunc() {
                    var int x;
                    var int y;
                    x = y.attr;
                    y.attr = x;
                    
                    x = Proto.a;
                    x = Proto;
                    
                    x = NonExistant.a;

                    var int new_x1 = y.attr;
                    var int new_x2 = Proto.a;
                    var int new_x3 = Proto;
                    var int new_x4 = NonExistant.a;
                    var int new_x5 = x;
                    var int new_x6 = y;

                    var int tab[6] = {y.attr, Proto.a, Proto, NonExistant.a, x, y};
                };
            ";

            ExpectedCompilationOutput = @"
                test.d: In prototype 'Proto':
                test.d:3:16: error: 'X' undeclared
                prototype Proto(X) {};
                                ^
                test.d: In function 'testFunc':
                test.d:8:10: error: cannot access attribute 'attr' because 'y' is not an instance of a class
                    x = y.attr;
                          ^
                test.d:9:6: error: cannot access attribute 'attr' because 'y' is not an instance of a class
                    y.attr = x;
                      ^
                test.d:11:14: error: cannot access attribute 'a' because 'Proto' is not an instance of a class
                    x = Proto.a;
                              ^
                test.d:14:8: error: 'NonExistant' undeclared
                    x = NonExistant.a;
                        ^
                test.d:16:23: error: cannot access attribute 'attr' because 'y' is not an instance of a class
                    var int new_x1 = y.attr;
                                       ^
                test.d:17:27: error: cannot access attribute 'a' because 'Proto' is not an instance of a class
                    var int new_x2 = Proto.a;
                                           ^
                test.d:19:21: error: 'NonExistant' undeclared
                    var int new_x4 = NonExistant.a;
                                     ^
                test.d:23:24: error: cannot access attribute 'attr' because 'y' is not an instance of a class
                    var int tab[6] = {y.attr, Proto.a, Proto, NonExistant.a, x, y};
                                        ^
                test.d:23:36: error: cannot access attribute 'a' because 'Proto' is not an instance of a class
                    var int tab[6] = {y.attr, Proto.a, Proto, NonExistant.a, x, y};
                                                    ^
                test.d:23:46: error: 'NonExistant' undeclared
                    var int tab[6] = {y.attr, Proto.a, Proto, NonExistant.a, x, y};
                                                              ^
                11 errors generated.
                ";

            AssertCompilationOutputMatch();
        }
    }
}