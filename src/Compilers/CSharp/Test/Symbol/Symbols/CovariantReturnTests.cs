﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.Symbols
{
    public class CovariantReturnTests : CSharpTestBase
    {
        private static void VerifyOverride(CSharpCompilation comp, string methodName, string overridingMethodDisplay, string overriddenMethodDisplay)
        {
            var method = comp.GlobalNamespace.GetMember(methodName);
            Assert.Equal(overridingMethodDisplay, method.ToTestDisplayString());
            var overridden = method.GetOverriddenMember();
            Assert.Equal(overriddenMethodDisplay, overridden.ToTestDisplayString());
        }

        private static void VerifyNoOverride(CSharpCompilation comp, string methodName)
        {
            var method = comp.GlobalNamespace.GetMember(methodName);
            var overridden = method.GetOverriddenMember();
            Assert.Null(overridden);
        }

        private static CSharpCompilation MetadataView(CSharpCompilation comp, params MetadataReference[] references)
        {
            references = references.Append(comp.ToMetadataReference());
            return CreateCompilation("", references: references);
        }

        [Fact]
        public void CovariantReturns_01()
        {
            var source = @"
public class Base
{
    public virtual object M() => null;
}
public class Derived : Base
{
    public override string M() => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M() => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(8, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "System.String Derived.M()", "System.Object Base.M()");
            }
        }

        [Fact]
        public void CovariantReturns_02()
        {
            var source = @"
public class Base
{
    public virtual T M<T, U>() where T : class where U : class, T => null;
}
public class Derived : Base
{
    public override U M<T, U>() => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,23): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override U M<T, U>() => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(8, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "U Derived.M<T, U>()", "T Base.M<T, U>()");
            }
        }

        [Fact]
        public void CovariantReturns_03()
        {
            var source = @"
public class Base<T> where T : class
{
    public virtual T M() => null;
}
public class Derived<T, U> : Base<T> where T : class where U : class, T
{
    public override U M() => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,23): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override U M() => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(8, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "U Derived<T, U>.M()", "T Base<T>.M()");
            }
        }

        [Fact]
        public void CovariantReturns_04()
        {
            var source = @"
public class N { }
public class Base
{
    public virtual N M() => null;
}
public class Derived<T> : Base where T : N
{
    public override T M() => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (9,23): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override T M() => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(9, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "T Derived<T>.M()", "N Base.M()");
            }
        }

        [Fact]
        public void CovariantReturns_05()
        {
            var source = @"
public class Base
{
    public virtual object M => null;
}
public class Derived : Base
{
    public override string M => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(8, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "System.String Derived.M { get; }", "System.Object Base.M { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_06()
        {
            var source = @"
public class Base<T> where T : class
{
    public virtual T M => null;
}
public class Derived<T, U> : Base<T> where T : class where U : class, T
{
    public override U M => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,23): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override U M => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(8, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "U Derived<T, U>.M { get; }", "T Base<T>.M { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_07()
        {
            var source = @"
public class N { }
public class Base
{
    public virtual N M => null;
}
public class Derived<T> : Base where T : N
{
    public override T M => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (9,23): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override T M => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(9, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "T Derived<T>.M { get; }", "N Base.M { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_08()
        {
            var source = @"
public class Base
{
    public virtual object this[int i] => null;
}
public class Derived : Base
{
    public override string this[int i] => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string this[int i] => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "this").WithArguments("covariant returns").WithLocation(8, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.this[]", "System.String Derived.this[System.Int32 i] { get; }", "System.Object Base.this[System.Int32 i] { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_09()
        {
            var source = @"
public class Base<T> where T : class
{
    public virtual T this[int i] => null;
}
public class Derived<T, U> : Base<T> where T : class where U : class, T
{
    public override U this[int i] => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,23): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override U this[int i] => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "this").WithArguments("covariant returns").WithLocation(8, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.this[]", "U Derived<T, U>.this[System.Int32 i] { get; }", "T Base<T>.this[System.Int32 i] { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_10()
        {
            var source = @"
public class N { }
public class Base
{
    public virtual N this[int i] => null;
}
public class Derived<T> : Base where T : N
{
    public override T this[int i] => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (9,23): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override T this[int i] => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "this").WithArguments("covariant returns").WithLocation(9, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.this[]", "T Derived<T>.this[System.Int32 i] { get; }", "N Base.this[System.Int32 i] { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_Events()
        {
            var source = @"
using System;
public class Base
{
    public virtual event Func<object> E;
    private void SuppressUnusedWarning() => E?.Invoke();
}
public class Derived : Base
{
    public override event Func<string> E;
    private void SuppressUnusedWarning() => E?.Invoke();
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (10,40): error CS1715: 'Derived.E': type must be 'Func<object>' to match overridden member 'Base.E'
                //     public override event Func<string> E;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "E").WithArguments("Derived.E", "Base.E", "System.Func<object>").WithLocation(10, 40)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (10,40): error CS1715: 'Derived.E': type must be 'Func<object>' to match overridden member 'Base.E'
                //     public override event Func<string> E;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "E").WithArguments("Derived.E", "Base.E", "System.Func<object>").WithLocation(10, 40)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.E", "event System.Func<System.String> Derived.E", "event System.Func<System.Object> Base.E");
            }
        }

        [Fact]
        public void CovariantReturns_WritableProperties()
        {
            var source = @"
using System;
public class Base
{
    public virtual Func<object> P { get; set; }
}
public class Derived : Base
{
    public override Func<string> P { get; set; }
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (9,34): error CS1715: 'Derived.P': type must be 'Func<object>' to match overridden member 'Base.P'
                //     public override Func<string> P { get; set; }
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "P").WithArguments("Derived.P", "Base.P", "System.Func<object>").WithLocation(9, 34)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (9,34): error CS1715: 'Derived.P': type must be 'Func<object>' to match overridden member 'Base.P'
                //     public override Func<string> P { get; set; }
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "P").WithArguments("Derived.P", "Base.P", "System.Func<object>").WithLocation(9, 34)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.P", "System.Func<System.String> Derived.P { get; set; }", "System.Func<System.Object> Base.P { get; set; }");
                VerifyNoOverride(comp, "Derived.set_P");
                VerifyOverride(comp, "Derived.get_P", "System.Func<System.String> Derived.P.get", "System.Func<System.Object> Base.P.get");
            }
        }

        [Fact]
        public void CovariantReturns_MetadataVsSource_01()
        {
            var s0 = @"
public class Base
{
    public virtual object M() => null;
}
";
            var baseMetadata = CreateCompilation(s0).EmitToImageReference();
            var source = @"
public class Derived : Base
{
    public override string M() => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns, references: new[] { baseMetadata }).VerifyDiagnostics(
                // (4,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M() => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(4, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns, references: new[] { baseMetadata }).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp, baseMetadata));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "System.String Derived.M()", "System.Object Base.M()");
            }
        }

        [Fact]
        public void CovariantReturns_MetadataVsSource_02()
        {
            var s0 = @"
public class Base
{
    public virtual object M => null;
}
";
            var baseMetadata = CreateCompilation(s0).EmitToImageReference();
            var source = @"
public class Derived : Base
{
    public override string M => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns, references: new[] { baseMetadata }).VerifyDiagnostics(
                // (4,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(4, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns, references: new[] { baseMetadata }).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp, baseMetadata));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "System.String Derived.M { get; }", "System.Object Base.M { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_MetadataVsSource_03()
        {
            var s0 = @"
public class Base
{
    public virtual object this[int i] => null;
}
";
            var baseMetadata = CreateCompilation(s0).EmitToImageReference();
            var source = @"
public class Derived : Base
{
    public override string this[int i] => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns, references: new[] { baseMetadata }).VerifyDiagnostics(
                // (4,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string this[int i] => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "this").WithArguments("covariant returns").WithLocation(4, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns, references: new[] { baseMetadata }).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp, baseMetadata));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.this[]", "System.String Derived.this[System.Int32 i] { get; }", "System.Object Base.this[System.Int32 i] { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_11()
        {
            var source = @"
public abstract class Base
{
    public abstract object M();
}
public class Derived : Base
{
    public override string M() => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M() => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(8, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "System.String Derived.M()", "System.Object Base.M()");
            }
        }

        [Fact]
        public void CovariantReturns_WrongReturnType()
        {
            var source = @"
public class Base
{
    public virtual string M() => null;
}
public class Derived : Base
{
    public override object M() => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS0508: 'Derived.M()': return type must be 'string' to match overridden member 'Base.M()'
                //     public override object M() => null;
                Diagnostic(ErrorCode.ERR_CantChangeReturnTypeOnOverride, "M").WithArguments("Derived.M()", "Base.M()", "string").WithLocation(8, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS0508: 'Derived.M()': return type must be 'string' to match overridden member 'Base.M()'
                //     public override object M() => null;
                Diagnostic(ErrorCode.ERR_CantChangeReturnTypeOnOverride, "M").WithArguments("Derived.M()", "Base.M()", "string").WithLocation(8, 28)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "System.Object Derived.M()", "System.String Base.M()");
            }
        }

        [Fact]
        public void NonOverrideTests_01()
        {
            var source = @"
public class Base
{
    public virtual object M1 => null;
    public virtual object M2 => null;
}
public class Derived : Base
{
    public new string M1 => null;
    public string M2 => null;    // A
}
public class Derived2 : Derived
{
    public new string M1 => null;
    public string M2 => null;    // B
}
public class Derived3 : Derived
{
    public new object M1 => null;
    public object M2 => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (10,19): warning CS0114: 'Derived.M2' hides inherited member 'Base.M2'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword.
                //     public string M2 => null;    // A
                Diagnostic(ErrorCode.WRN_NewOrOverrideExpected, "M2").WithArguments("Derived.M2", "Base.M2").WithLocation(10, 19),
                // (15,19): warning CS0108: 'Derived2.M2' hides inherited member 'Derived.M2'. Use the new keyword if hiding was intended.
                //     public string M2 => null;    // B
                Diagnostic(ErrorCode.WRN_NewRequired, "M2").WithArguments("Derived2.M2", "Derived.M2").WithLocation(15, 19),
                // (20,19): warning CS0108: 'Derived3.M2' hides inherited member 'Derived.M2'. Use the new keyword if hiding was intended.
                //     public object M2 => null;
                Diagnostic(ErrorCode.WRN_NewRequired, "M2").WithArguments("Derived3.M2", "Derived.M2").WithLocation(20, 19)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (10,19): warning CS0114: 'Derived.M2' hides inherited member 'Base.M2'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword.
                //     public string M2 => null;    // A
                Diagnostic(ErrorCode.WRN_NewOrOverrideExpected, "M2").WithArguments("Derived.M2", "Base.M2").WithLocation(10, 19),
                // (15,19): warning CS0108: 'Derived2.M2' hides inherited member 'Derived.M2'. Use the new keyword if hiding was intended.
                //     public string M2 => null;    // B
                Diagnostic(ErrorCode.WRN_NewRequired, "M2").WithArguments("Derived2.M2", "Derived.M2").WithLocation(15, 19),
                // (20,19): warning CS0108: 'Derived3.M2' hides inherited member 'Derived.M2'. Use the new keyword if hiding was intended.
                //     public object M2 => null;
                Diagnostic(ErrorCode.WRN_NewRequired, "M2").WithArguments("Derived3.M2", "Derived.M2").WithLocation(20, 19)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyNoOverride(comp, "Derived.M1");
                VerifyNoOverride(comp, "Derived.M2");
                VerifyNoOverride(comp, "Derived2.M1");
                VerifyNoOverride(comp, "Derived2.M2");
                VerifyNoOverride(comp, "Derived3.M1");
                VerifyNoOverride(comp, "Derived3.M2");
            }
        }

        [Fact]
        public void ChainedOverrides_01()
        {
            var source = @"
public class Base
{
    public virtual object M1 => null;
    public virtual object M2 => null;
    public virtual object M3 => null;
}
public class Derived : Base
{
    public override string M1 => null; // A
    public override string M2 => null; // B
    public override string M3 => null; // C
}
public class Derived2 : Derived
{
    public override string M1 => null;
    public override object M2 => null; // 1
    public override Base M3 => null;   // 2
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (10,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M1 => null; // A
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M1").WithArguments("covariant returns").WithLocation(10, 28),
                // (11,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M2 => null; // B
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M2").WithArguments("covariant returns").WithLocation(11, 28),
                // (12,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M3 => null; // C
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M3").WithArguments("covariant returns").WithLocation(12, 28),
                // (17,28): error CS1715: 'Derived2.M2': type must be 'string' to match overridden member 'Derived.M2'
                //     public override object M2 => null; // 1
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M2").WithArguments("Derived2.M2", "Derived.M2", "string").WithLocation(17, 28),
                // (18,26): error CS1715: 'Derived2.M3': type must be 'string' to match overridden member 'Derived.M3'
                //     public override Base M3 => null;   // 2
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M3").WithArguments("Derived2.M3", "Derived.M3", "string").WithLocation(18, 26)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (17,28): error CS1715: 'Derived2.M2': type must be 'string' to match overridden member 'Derived.M2'
                //     public override object M2 => null; // 1
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M2").WithArguments("Derived2.M2", "Derived.M2", "string").WithLocation(17, 28),
                // (18,26): error CS1715: 'Derived2.M3': type must be 'string' to match overridden member 'Derived.M3'
                //     public override Base M3 => null;   // 2
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M3").WithArguments("Derived2.M3", "Derived.M3", "string").WithLocation(18, 26)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M1", "System.String Derived.M1 { get; }", "System.Object Base.M1 { get; }");
                VerifyOverride(comp, "Derived.M2", "System.String Derived.M2 { get; }", "System.Object Base.M2 { get; }");
                VerifyOverride(comp, "Derived.M3", "System.String Derived.M3 { get; }", "System.Object Base.M3 { get; }");
                VerifyOverride(comp, "Derived2.M1", "System.String Derived2.M1 { get; }", "System.String Derived.M1 { get; }");
                VerifyOverride(comp, "Derived2.M2", "System.Object Derived2.M2 { get; }", "System.String Derived.M2 { get; }");
                VerifyOverride(comp, "Derived2.M3", "Base Derived2.M3 { get; }", "System.String Derived.M3 { get; }");
            }
        }

        [Fact]
        public void NestedVariance_01()
        {
            var source = @"
public class Base
{
    public virtual IIn<string> M1 => null;
    public virtual IOut<object> M2 => null;
}
public class Derived : Base
{
    public override IIn<object> M1 => null;
    public override IOut<string> M2 => null;
}
public interface IOut<out T> { }
public interface IIn<in T> { }
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (9,33): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override IIn<object> M1 => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M1").WithArguments("covariant returns").WithLocation(9, 33),
                // (10,34): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override IOut<string> M2 => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M2").WithArguments("covariant returns").WithLocation(10, 34)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M1", "IIn<System.Object> Derived.M1 { get; }", "IIn<System.String> Base.M1 { get; }");
                VerifyOverride(comp, "Derived.M2", "IOut<System.String> Derived.M2 { get; }", "IOut<System.Object> Base.M2 { get; }");
            }
        }

        [Fact]
        public void NestedVariance_02()
        {
            var source = @"
public class Base
{
    public virtual IIn<object> M1 => null;
    public virtual IOut<string> M2 => null;
}
public class Derived : Base
{
    public override IIn<string> M1 => null;
    public override IOut<object> M2 => null;
}
public interface IOut<out T> { }
public interface IIn<in T> { }
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (9,33): error CS1715: 'Derived.M1': type must be 'IIn<object>' to match overridden member 'Base.M1'
                //     public override IIn<string> M1 => null;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M1").WithArguments("Derived.M1", "Base.M1", "IIn<object>").WithLocation(9, 33),
                // (10,34): error CS1715: 'Derived.M2': type must be 'IOut<string>' to match overridden member 'Base.M2'
                //     public override IOut<object> M2 => null;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M2").WithArguments("Derived.M2", "Base.M2", "IOut<string>").WithLocation(10, 34)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (9,33): error CS1715: 'Derived.M1': type must be 'IIn<object>' to match overridden member 'Base.M1'
                //     public override IIn<string> M1 => null;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M1").WithArguments("Derived.M1", "Base.M1", "IIn<object>").WithLocation(9, 33),
                // (10,34): error CS1715: 'Derived.M2': type must be 'IOut<string>' to match overridden member 'Base.M2'
                //     public override IOut<object> M2 => null;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M2").WithArguments("Derived.M2", "Base.M2", "IOut<string>").WithLocation(10, 34)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M1", "IIn<System.String> Derived.M1 { get; }", "IIn<System.Object> Base.M1 { get; }");
                VerifyOverride(comp, "Derived.M2", "IOut<System.Object> Derived.M2 { get; }", "IOut<System.String> Base.M2 { get; }");
            }
        }

        [Fact]
        public void BadCovariantReturnType_01()
        {
            var source = @"
public class Base
{
    public virtual int M1 => 1;
    public virtual A M2 => null;
}
public class Derived : Base
{
    public override short M1 => 1;
    public override B M2 => null;
}
public class A { }
public class B
{
    public static implicit operator A(B b) => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (9,27): error CS1715: 'Derived.M1': type must be 'int' to match overridden member 'Base.M1'
                //     public override short M1 => 1;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M1").WithArguments("Derived.M1", "Base.M1", "int").WithLocation(9, 27),
                // (10,23): error CS1715: 'Derived.M2': type must be 'A' to match overridden member 'Base.M2'
                //     public override B M2 => null;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M2").WithArguments("Derived.M2", "Base.M2", "A").WithLocation(10, 23)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (9,27): error CS1715: 'Derived.M1': type must be 'int' to match overridden member 'Base.M1'
                //     public override short M1 => 1;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M1").WithArguments("Derived.M1", "Base.M1", "int").WithLocation(9, 27),
                // (10,23): error CS1715: 'Derived.M2': type must be 'A' to match overridden member 'Base.M2'
                //     public override B M2 => null;
                Diagnostic(ErrorCode.ERR_CantChangeTypeOnOverride, "M2").WithArguments("Derived.M2", "Base.M2", "A").WithLocation(10, 23)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M1", "System.Int16 Derived.M1 { get; }", "System.Int32 Base.M1 { get; }");
                VerifyOverride(comp, "Derived.M2", "B Derived.M2 { get; }", "A Base.M2 { get; }");
            }
        }

        [Fact]
        public void CovariantReturns_12()
        {
            var source = @"
public class Base
{
    public virtual System.IComparable M => null;
}
public class Derived : Base
{
    public override string M => null;
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string M => null;
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "M").WithArguments("covariant returns").WithLocation(8, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.M", "System.String Derived.M { get; }", "System.IComparable Base.M { get; }");
            }
        }

        [Fact]
        public void NoCovariantImplementations_01()
        {
            var source = @"
public interface Base
{
    public virtual object M1 => null;
    public virtual object M2() => null;
}
public interface Derived : Base
{
    string Base.M1 => null;   // 1
    string Base.M2() => null; // 2
}
public class C : Base
{
    string Base.M1 => null;   // 3
    string Base.M2() => null; // 4
}
";
            // these are poor diagnostics; see https://github.com/dotnet/roslyn/issues/43719
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns, targetFramework: TargetFramework.NetStandardLatest).VerifyDiagnostics(
                // (9,17): error CS0539: 'Derived.M1' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M1 => null;   // 1
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M1").WithArguments("Derived.M1").WithLocation(9, 17),
                // (10,17): error CS0539: 'Derived.M2()' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M2() => null; // 2
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M2").WithArguments("Derived.M2()").WithLocation(10, 17),
                // (14,17): error CS0539: 'C.M1' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M1 => null;   // 3
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M1").WithArguments("C.M1").WithLocation(14, 17),
                // (15,17): error CS0539: 'C.M2()' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M2() => null; // 4
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M2").WithArguments("C.M2()").WithLocation(15, 17)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns, targetFramework: TargetFramework.NetStandardLatest).VerifyDiagnostics(
                // (9,17): error CS0539: 'Derived.M1' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M1 => null;   // 1
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M1").WithArguments("Derived.M1").WithLocation(9, 17),
                // (10,17): error CS0539: 'Derived.M2()' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M2() => null; // 2
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M2").WithArguments("Derived.M2()").WithLocation(10, 17),
                // (14,17): error CS0539: 'C.M1' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M1 => null;   // 3
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M1").WithArguments("C.M1").WithLocation(14, 17),
                // (15,17): error CS0539: 'C.M2()' in explicit interface declaration is not found among members of the interface that can be implemented
                //     string Base.M2() => null; // 4
                Diagnostic(ErrorCode.ERR_InterfaceMemberNotFound, "M2").WithArguments("C.M2()").WithLocation(15, 17)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyNoOverride(comp, "Derived.Base.M1");
                VerifyNoOverride(comp, "Derived.Base.M2");
                VerifyNoOverride(comp, "C.Base.M1");
                VerifyNoOverride(comp, "C.Base.M2");
            }
        }

        [Fact]
        public void CovariantReturns_13()
        {
            var source = @"
public class Base
{
    public virtual object P { get; set; }
}
public class Derived : Base
{
    public override string P { get => string.Empty; }
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string P { get; }
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "P").WithArguments("covariant returns").WithLocation(8, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.P", "System.String Derived.P { get; }", "System.Object Base.P { get; set; }");
                VerifyOverride(comp, "Derived.get_P", "System.String Derived.P.get", "System.Object Base.P.get");
            }
        }

        [Fact]
        public void CovariantReturns_14()
        {
            var source = @"
public class Base
{
    public virtual object P { get; set; }
}
public class Derived : Base
{
    public override string P { get => string.Empty; }
}
public class Derived2 : Derived
{
    public override string P { get => string.Empty; set { } }
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string P { get => string.Empty; }
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "P").WithArguments("covariant returns").WithLocation(8, 28),
                // (12,53): error CS0546: 'Derived2.P.set': cannot override because 'Derived.P' does not have an overridable set accessor
                //     public override string P { get => string.Empty; set { } }
                Diagnostic(ErrorCode.ERR_NoSetToOverride, "set").WithArguments("Derived2.P.set", "Derived.P").WithLocation(12, 53)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (12,53): error CS0546: 'Derived2.P.set': cannot override because 'Derived.P' does not have an overridable set accessor
                //     public override string P { get => string.Empty; set { } }
                Diagnostic(ErrorCode.ERR_NoSetToOverride, "set").WithArguments("Derived2.P.set", "Derived.P").WithLocation(12, 53)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.P", "System.String Derived.P { get; }", "System.Object Base.P { get; set; }");
                VerifyOverride(comp, "Derived.get_P", "System.String Derived.P.get", "System.Object Base.P.get");
                VerifyOverride(comp, "Derived2.P", "System.String Derived2.P { get; set; }", "System.String Derived.P { get; }");
                VerifyOverride(comp, "Derived2.get_P", "System.String Derived2.P.get", "System.String Derived.P.get");
                VerifyNoOverride(comp, "Derived2.set_P");
            }
        }

        [Fact]
        public void CovariantReturns_15()
        {
            var source = @"
public class Base
{
    public virtual object P { get; set; }
}
public class Derived : Base
{
    public override string P { get => string.Empty; }
}
public class Derived2 : Derived
{
    public override string P { set { } }
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string P { get => string.Empty; }
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "P").WithArguments("covariant returns").WithLocation(8, 28),
                // (12,32): error CS0546: 'Derived2.P.set': cannot override because 'Derived.P' does not have an overridable set accessor
                //     public override string P { set { } }
                Diagnostic(ErrorCode.ERR_NoSetToOverride, "set").WithArguments("Derived2.P.set", "Derived.P").WithLocation(12, 32)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                // (12,32): error CS0546: 'Derived2.P.set': cannot override because 'Derived.P' does not have an overridable set accessor
                //     public override string P { set { } }
                Diagnostic(ErrorCode.ERR_NoSetToOverride, "set").WithArguments("Derived2.P.set", "Derived.P").WithLocation(12, 32)
                );
            verify(comp);

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.P", "System.String Derived.P { get; }", "System.Object Base.P { get; set; }");
                VerifyOverride(comp, "Derived.get_P", "System.String Derived.P.get", "System.Object Base.P.get");
                VerifyOverride(comp, "Derived2.P", "System.String Derived2.P { set; }", "System.String Derived.P { get; }");
                VerifyNoOverride(comp, "Derived2.set_P");
            }
        }

        [Fact]
        public void CovariantReturns_16()
        {
            var source = @"
public class Base
{
    public virtual object P { get; set; }
}
public class Derived : Base
{
    public override System.IComparable P { get => string.Empty; }
}
public class Derived2 : Derived
{
    public override string P { get => string.Empty; }
}
";
            var comp = CreateCompilation(source, parseOptions: TestOptions.WithoutCovariantReturns).VerifyDiagnostics(
                // (8,40): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override System.IComparable P { get => string.Empty; }
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "P").WithArguments("covariant returns").WithLocation(8, 40),
                // (12,28): error CS8652: The feature 'covariant returns' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override string P { get => string.Empty; }
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "P").WithArguments("covariant returns").WithLocation(12, 28)
                );
            verify(comp);
            comp = CreateCompilation(source, parseOptions: TestOptions.WithCovariantReturns).VerifyDiagnostics(
                );
            verify(comp);
            verify(MetadataView(comp));

            static void verify(CSharpCompilation comp)
            {
                VerifyOverride(comp, "Derived.P", "System.IComparable Derived.P { get; }", "System.Object Base.P { get; set; }");
                VerifyOverride(comp, "Derived.get_P", "System.IComparable Derived.P.get", "System.Object Base.P.get");
                VerifyOverride(comp, "Derived2.P", "System.String Derived2.P { get; }", "System.IComparable Derived.P { get; }");
                VerifyOverride(comp, "Derived2.get_P", "System.String Derived2.P.get", "System.IComparable Derived.P.get");
            }
        }
    }
}
