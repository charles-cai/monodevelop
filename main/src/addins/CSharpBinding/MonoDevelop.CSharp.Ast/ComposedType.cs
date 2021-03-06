// 
// ComposedType.cs
//
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoDevelop.CSharp.Ast
{
	public class ComposedType : AstType
	{
		public static readonly Role<CSharpTokenNode> NullableRole = new Role<CSharpTokenNode>("Nullable", CSharpTokenNode.Null);
		public static readonly Role<CSharpTokenNode> PointerRole = new Role<CSharpTokenNode>("Pointer", CSharpTokenNode.Null);
		public static readonly Role<ArraySpecifier> ArraySpecifierRole = new Role<ArraySpecifier>("ArraySpecifier");
		
		public AstType BaseType {
			get { return GetChildByRole(Roles.Type); }
			set { SetChildByRole(Roles.Type, value); }
		}
		
		public bool HasNullableSpecifier {
			get {
				return !GetChildByRole(NullableRole).IsNull;
			}
			set {
				SetChildByRole(NullableRole, value ? new CSharpTokenNode(AstLocation.Empty, 1) : null);
			}
		}
		
		public int PointerRank {
			get {
				return GetChildrenByRole(PointerRole).Count();
			}
			set {
				// remove old children
				foreach (AstNode node in GetChildrenByRole(PointerRole))
					node.Remove();
				// add new children
				for (int i = 0; i < value; i++) {
					AddChild(new CSharpTokenNode(AstLocation.Empty, 1), PointerRole);
				}
			}
		}
		
		public IEnumerable<ArraySpecifier> ArraySpecifiers {
			get { return GetChildrenByRole (ArraySpecifierRole); }
			set { SetChildrenByRole (ArraySpecifierRole, value); }
		}
		
		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitComposedType (this, data);
		}
		
		public override string ToString()
		{
			StringBuilder b = new StringBuilder();
			b.Append(this.BaseType.ToString());
			if (this.HasNullableSpecifier)
				b.Append('?');
			b.Append('*', this.PointerRank);
			foreach (var arraySpecifier in this.ArraySpecifiers) {
				b.Append('[');
				b.Append(',', arraySpecifier.Dimensions - 1);
				b.Append(']');
			}
			return b.ToString();
		}
	}
	
	/// <summary>
	/// [,,,]
	/// </summary>
	public class ArraySpecifier : AstNode
	{
		public override NodeType NodeType {
			get {
				return NodeType.Unknown;
			}
		}
		
		public CSharpTokenNode LBracketToken {
			get { return GetChildByRole (Roles.LBracket); }
		}
		
		public int Dimensions {
			get { return 1 + GetChildrenByRole(Roles.Comma).Count(); }
			set {
				int d = this.Dimensions;
				while (d > value) {
					GetChildByRole(Roles.Comma).Remove();
					d--;
				}
				while (d < value) {
					InsertChildBefore(GetChildByRole(Roles.Comma), new CSharpTokenNode(AstLocation.Empty, 1), Roles.Comma);
					d++;
				}
			}
		}
		
		public CSharpTokenNode RBracketToken {
			get { return GetChildByRole (Roles.RBracket); }
		}
		
		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitArraySpecifier(this, data);
		}
		
		public override string ToString()
		{
			return "[" + new string(',', this.Dimensions - 1) + "]";
		}
	}
}

