using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Resolving
{
	internal class ManyWeightedResolved<TResolved>
	{
		List<TResolved> _resolved = new List<TResolved>();
		Dictionary<Type, int> _weights = new Dictionary<Type, int>();

		public ManyWeightedResolved()
		{
			Resolution.Frozen += (sender, e) =>
				{
					_resolved = _resolved.OrderBy(r => _weights[r.GetType()]).ToList();
					_weights = null;
				};
		}

		public ManyWeightedResolved(IEnumerable<TResolved> resolved)
			: this()
		{
			_resolved.AddRange(resolved);
			foreach (var type in _resolved.Select(r => r.GetType()))
				_weights.Add(type, ResolutionWeightAttribute.ReadWeight(type));
		}

		public IEnumerable<TResolved> Values
		{
			get { return _resolved; }
		}

		#region Manage collection

		public void Add(TResolved value)
		{
			Resolution.EnsureNotFrozen();

			var type = value.GetType();
			EnsureNotExists(type);
			_resolved.Add(value);
			_weights[type] = ResolutionWeightAttribute.ReadWeight(type);
		}

		public void Add(TResolved value, int weight)
		{
			Resolution.EnsureNotFrozen();

			var type = value.GetType();
			EnsureNotExists(type);
			_resolved.Add(value);
			_weights[type] = weight;
		}

		public void AddRange(IEnumerable<TResolved> values)
		{
			Resolution.EnsureNotFrozen();

			foreach (var value in values)
			{
				var type = value.GetType();
				EnsureNotExists(type);
				_resolved.Add(value);
				_weights[type] = ResolutionWeightAttribute.ReadWeight(type);
			}
		}

		//public void SetWeight(TResolved value, int weight)
		//{
		//    Resolution.EnsureNotFrozen();

		//    var type = value.GetType();
		//    EnsureExists(type);
		//    _weights[type] = weight;
		//}

		public void SetWeight<TResolving>(int weight)
		{
			Resolution.EnsureNotFrozen();

			var type = typeof(TResolving);
			EnsureExists(type);
			_weights[type] = weight;
		}

		//public int GetWeight(TResolved value)
		//{
		//    var type = value.GetType();
		//    EnsureExists(type);
		//    return _weights[value.GetType()];
		//}

		public int GetWeight<TResolving>()
		{
			var type = typeof(TResolving);
			EnsureExists(type);
			return _weights[type];
		}

		//public void Remove(TResolved value)
		//{
		//    Resolution.EnsureNotFrozen();

		//    var type = value.GetType();
		//    var remove = _resolved.SingleOrDefault(r => r.GetType() == type);
		//    if (remove != null)
		//    {
		//        _resolved.Remove(remove);
		//        _weights.Remove(remove.GetType());
		//    }
		//}

		public void Remove<TResolving>()
		{
			Resolution.EnsureNotFrozen();

			var type = typeof(TResolving);
			var remove = _resolved.SingleOrDefault(r => r.GetType() == type);
			if (remove != null)
			{
				_resolved.Remove(remove);
				_weights.Remove(remove.GetType());
			}
		}

		public void Clear()
		{
			Resolution.EnsureNotFrozen();

			_resolved = new List<TResolved>();
			_weights = new Dictionary<Type, int>();
		}

		#endregion

		#region Utilities

		public bool Exists(Type type)
		{
			return _resolved.Any(r => r.GetType() == type);
		}

		void EnsureExists(Type type)
		{
			if (!Exists(type))
				throw new InvalidOperationException("There is not value of that type in the collection.");
		}

		void EnsureNotExists(Type type)
		{
			if (Exists(type))
				throw new InvalidOperationException("A value of that type already exists in the collection.");
		}

		#endregion
	}
}
