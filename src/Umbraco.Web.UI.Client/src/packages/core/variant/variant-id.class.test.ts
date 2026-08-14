import { expect } from '@open-wc/testing';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from './variant-id.class.js';

describe('UmbVariantId', () => {
	describe('constructor', () => {
		it('defaults culture and segment to null when called with no arguments', () => {
			const id = new UmbVariantId();
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal(null);
		});

		it('normalizes the "invariant" culture literal to null', () => {
			const id = new UmbVariantId(UMB_INVARIANT_CULTURE, 'xmas');
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal('xmas');
		});

		it('keeps non-invariant cultures and segments as given', () => {
			const id = new UmbVariantId('en', 'xmas');
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal('xmas');
		});

		it('coerces undefined inputs to null', () => {
			const id = new UmbVariantId(undefined, undefined);
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal(null);
		});
	});

	describe('INVARIANT', () => {
		it('is the same reference returned by CreateInvariant()', () => {
			expect(UmbVariantId.CreateInvariant()).to.equal(UmbVariantId.INVARIANT);
		});

		it('compares true against a freshly created invariant', () => {
			expect(UmbVariantId.INVARIANT.compare(UmbVariantId.CreateInvariant())).to.be.true;
		});

		it('has null culture and null segment', () => {
			expect(UmbVariantId.INVARIANT.culture).to.equal(null);
			expect(UmbVariantId.INVARIANT.segment).to.equal(null);
		});

		it('is frozen', () => {
			expect(Object.isFrozen(UmbVariantId.INVARIANT)).to.be.true;
		});
	});

	describe('Create', () => {
		it('creates a variant from a full culture/segment object', () => {
			const id = UmbVariantId.Create({ culture: 'en', segment: 'xmas' });
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal('xmas');
		});

		it('returns a frozen instance', () => {
			const id = UmbVariantId.Create({ culture: 'en', segment: null });
			expect(Object.isFrozen(id)).to.be.true;
		});

		it('returns a new instance on each call', () => {
			const a = UmbVariantId.Create({ culture: 'en', segment: null });
			const b = UmbVariantId.Create({ culture: 'en', segment: null });
			expect(a).to.not.equal(b);
			expect(a.equal(b)).to.be.true;
		});
	});

	describe('CreateFromPartial', () => {
		it('fills missing culture and segment with null', () => {
			const id = UmbVariantId.CreateFromPartial({});
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal(null);
		});

		it('keeps provided values', () => {
			const id = UmbVariantId.CreateFromPartial({ culture: 'da' });
			expect(id.culture).to.equal('da');
			expect(id.segment).to.equal(null);
		});

		it('returns a frozen instance', () => {
			const id = UmbVariantId.CreateFromPartial({ culture: 'da' });
			expect(Object.isFrozen(id)).to.be.true;
		});
	});

	describe('FromString', () => {
		it('parses a culture-only string', () => {
			const id = UmbVariantId.FromString('en');
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal(null);
		});

		it('parses a culture+segment string', () => {
			const id = UmbVariantId.FromString('en_xmas');
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal('xmas');
		});

		it('treats the "invariant" culture literal as null culture', () => {
			const id = UmbVariantId.FromString('invariant');
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal(null);
		});

		it('keeps segment when culture is the invariant literal', () => {
			const id = UmbVariantId.FromString('invariant_xmas');
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal('xmas');
		});

		it('treats an empty segment after the separator as null', () => {
			const id = UmbVariantId.FromString('en_');
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal(null);
		});

		it('splits only on the first underscore', () => {
			const id = UmbVariantId.FromString('en_seg_ment');
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal('seg_ment');
		});

		it('returns a frozen instance', () => {
			const id = UmbVariantId.FromString('en_xmas');
			expect(Object.isFrozen(id)).to.be.true;
		});

		it('round-trips through toString()', () => {
			const cases = ['en', 'en_xmas', 'invariant', 'invariant_xmas'];
			cases.forEach((input) => {
				expect(UmbVariantId.FromString(input).toString()).to.equal(input);
			});
		});
	});

	describe('compare', () => {
		it('returns true for equal culture and segment objects', () => {
			const id = new UmbVariantId('en', 'xmas');
			expect(id.compare({ culture: 'en', segment: 'xmas' })).to.be.true;
		});

		it('returns false when culture differs', () => {
			const id = new UmbVariantId('en', null);
			expect(id.compare({ culture: 'da', segment: null })).to.be.false;
		});

		it('returns false when segment differs', () => {
			const id = new UmbVariantId('en', 'xmas');
			expect(id.compare({ culture: 'en', segment: null })).to.be.false;
		});

		it('treats the "invariant" culture literal as null culture when comparing', () => {
			const id = new UmbVariantId(null, null);
			expect(id.compare({ culture: UMB_INVARIANT_CULTURE, segment: null })).to.be.true;
		});
	});

	describe('equal', () => {
		it('returns true for two variants with matching culture and segment', () => {
			const a = new UmbVariantId('en', 'xmas');
			const b = new UmbVariantId('en', 'xmas');
			expect(a.equal(b)).to.be.true;
		});

		it('returns false when culture differs', () => {
			const a = new UmbVariantId('en', null);
			const b = new UmbVariantId('da', null);
			expect(a.equal(b)).to.be.false;
		});

		it('returns false when segment differs', () => {
			const a = new UmbVariantId('en', 'xmas');
			const b = new UmbVariantId('en', null);
			expect(a.equal(b)).to.be.false;
		});
	});

	describe('toString', () => {
		it('renders "invariant" when culture is null', () => {
			expect(new UmbVariantId(null, null).toString()).to.equal('invariant');
		});

		it('renders only the culture when segment is null', () => {
			expect(new UmbVariantId('en', null).toString()).to.equal('en');
		});

		it('appends the segment with an underscore when set', () => {
			expect(new UmbVariantId('en', 'xmas').toString()).to.equal('en_xmas');
		});

		it('renders "invariant_segment" for culture-invariant but segmented variants', () => {
			expect(new UmbVariantId(null, 'xmas').toString()).to.equal('invariant_xmas');
		});
	});

	describe('toCultureString', () => {
		it('returns the culture when set', () => {
			expect(new UmbVariantId('en', null).toCultureString()).to.equal('en');
		});

		it('returns "invariant" when culture is null', () => {
			expect(new UmbVariantId(null, 'xmas').toCultureString()).to.equal(UMB_INVARIANT_CULTURE);
		});
	});

	describe('toSegmentString', () => {
		it('returns the segment when set', () => {
			expect(new UmbVariantId('en', 'xmas').toSegmentString()).to.equal('xmas');
		});

		it('returns an empty string when segment is null', () => {
			expect(new UmbVariantId('en', null).toSegmentString()).to.equal('');
		});
	});

	describe('isCultureInvariant', () => {
		it('is true when culture is null', () => {
			expect(new UmbVariantId(null, 'xmas').isCultureInvariant()).to.be.true;
		});

		it('is false when culture is set', () => {
			expect(new UmbVariantId('en', null).isCultureInvariant()).to.be.false;
		});
	});

	describe('isSegmentInvariant', () => {
		it('is true when segment is null', () => {
			expect(new UmbVariantId('en', null).isSegmentInvariant()).to.be.true;
		});

		it('is false when segment is set', () => {
			expect(new UmbVariantId('en', 'xmas').isSegmentInvariant()).to.be.false;
		});
	});

	describe('isInvariant', () => {
		it('is true only when both culture and segment are null', () => {
			expect(new UmbVariantId(null, null).isInvariant()).to.be.true;
		});

		it('is false when culture is set', () => {
			expect(new UmbVariantId('en', null).isInvariant()).to.be.false;
		});

		it('is false when segment is set', () => {
			expect(new UmbVariantId(null, 'xmas').isInvariant()).to.be.false;
		});
	});

	describe('clone', () => {
		it('returns a new instance with the same culture and segment', () => {
			const source = new UmbVariantId('en', 'xmas');
			const copy = source.clone();
			expect(copy).to.not.equal(source);
			expect(copy.equal(source)).to.be.true;
		});
	});

	describe('toObject', () => {
		it('returns a plain object with culture and segment', () => {
			const id = new UmbVariantId('en', 'xmas');
			expect(id.toObject()).to.deep.equal({ culture: 'en', segment: 'xmas' });
		});

		it('returns null values when invariant', () => {
			const id = new UmbVariantId(null, null);
			expect(id.toObject()).to.deep.equal({ culture: null, segment: null });
		});
	});

	describe('toSegmentInvariant', () => {
		it('returns a new variant with the segment cleared', () => {
			const id = new UmbVariantId('en', 'xmas').toSegmentInvariant();
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal(null);
		});

		it('returns a frozen instance', () => {
			expect(Object.isFrozen(new UmbVariantId('en', 'xmas').toSegmentInvariant())).to.be.true;
		});
	});

	describe('toCultureInvariant', () => {
		it('returns a new variant with the culture cleared', () => {
			const id = new UmbVariantId('en', 'xmas').toCultureInvariant();
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal('xmas');
		});

		it('returns a frozen instance', () => {
			expect(Object.isFrozen(new UmbVariantId('en', 'xmas').toCultureInvariant())).to.be.true;
		});
	});

	describe('toCulture', () => {
		it('returns a new variant with the specified culture', () => {
			const id = new UmbVariantId('en', 'xmas').toCulture('da');
			expect(id.culture).to.equal('da');
			expect(id.segment).to.equal('xmas');
		});

		it('accepts null to clear the culture', () => {
			const id = new UmbVariantId('en', 'xmas').toCulture(null);
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal('xmas');
		});

		it('returns a frozen instance', () => {
			expect(Object.isFrozen(new UmbVariantId('en', null).toCulture('da'))).to.be.true;
		});
	});

	describe('toSegment', () => {
		it('returns a new variant with the specified segment', () => {
			const id = new UmbVariantId('en', null).toSegment('xmas');
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal('xmas');
		});

		it('accepts null to clear the segment', () => {
			const id = new UmbVariantId('en', 'xmas').toSegment(null);
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal(null);
		});

		it('returns a frozen instance', () => {
			expect(Object.isFrozen(new UmbVariantId('en', null).toSegment('xmas'))).to.be.true;
		});
	});

	describe('toVariant', () => {
		const source = new UmbVariantId('en', 'xmas');

		it('keeps culture and segment when both vary flags are true', () => {
			const id = source.toVariant(true, true);
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal('xmas');
		});

		it('clears the segment when varyBySegment is false', () => {
			const id = source.toVariant(true, false);
			expect(id.culture).to.equal('en');
			expect(id.segment).to.equal(null);
		});

		it('clears the culture when varyByCulture is false', () => {
			const id = source.toVariant(false, true);
			expect(id.culture).to.equal(null);
			expect(id.segment).to.equal('xmas');
		});

		it('returns an invariant when both vary flags are false or omitted', () => {
			expect(source.toVariant(false, false).isInvariant()).to.be.true;
			expect(source.toVariant().isInvariant()).to.be.true;
		});

		it('returns a frozen instance', () => {
			expect(Object.isFrozen(source.toVariant(true, true))).to.be.true;
		});
	});
});
