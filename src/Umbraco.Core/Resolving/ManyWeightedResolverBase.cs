using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Resolving
{
	//internal abstract class ManyWeightedResolverBase<TResolver, TResolved> : ResolverBase<TResolver> 
	//    where TResolver : class
	//{
	//    readonly ManyWeightedResolved<TResolved> _resolved;

	//    protected ManyWeightedResolverBase()
	//    {
	//        _resolved = new ManyWeightedResolved<TResolved>();
	//    }

	//    protected ManyWeightedResolverBase(IEnumerable<TResolved> values)
	//    {
	//        _resolved = new ManyWeightedResolved<TResolved>(values);
	//    }

	//    protected IEnumerable<TResolved> Values
	//    {
	//        get { return _resolved.Values; }
	//    }

	//    #region Manage collection

	//    public void Add(TResolved value)
	//    {
	//        _resolved.Add(value);
	//    }

	//    public void Add(TResolved value, int weight)
	//    {
	//        _resolved.Add(value, weight);
	//    }

	//    public void AddRange(IEnumerable<TResolved> values)
	//    {
	//        _resolved.AddRange(values);
	//    }

	//    public void SetWeight<TResolving>(int weight)
	//    {
	//        _resolved.SetWeight<TResolving>(weight);
	//    }

	//    public int GetWeight<TResolving>()
	//    {
	//        return _resolved.GetWeight<TResolving>();
	//    }

	//    public void Remove<TResolving>()
	//    {
	//        _resolved.Remove<TResolving>();
	//    }

	//    public void Clear()
	//    {
	//        _resolved.Clear();
	//    }

	//    #endregion
	//}
}
