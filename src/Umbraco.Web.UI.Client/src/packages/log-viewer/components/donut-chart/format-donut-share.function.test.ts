import { umbFormatDonutShare } from './format-donut-share.function.js';
import { expect } from '@open-wc/testing';

const localizeIn = (locale: string) => ({
	number: (value: number | string, options?: Intl.NumberFormatOptions) =>
		new Intl.NumberFormat(locale, options).format(Number(value)),
});

describe('umbFormatDonutShare', () => {
	const english = localizeIn('en');

	it('formats a share as a percentage with at most one decimal', () => {
		expect(umbFormatDonutShare(50000 / 51233, english)).to.equal('97.6%');
		expect(umbFormatDonutShare(0.25, english)).to.equal('25%');
	});

	it('shows zero and the whole as 0% and 100%', () => {
		expect(umbFormatDonutShare(0, english)).to.equal('0%');
		expect(umbFormatDonutShare(1, english)).to.equal('100%');
	});

	it('does not round a non-zero share down to 0%', () => {
		expect(umbFormatDonutShare(0.0004, english)).to.equal('<0.1%');
	});

	it('does not round a partial share up to 100%', () => {
		expect(umbFormatDonutShare(0.9996, english)).to.equal('>99.9%');
	});

	it('follows the number format of the localization controller', () => {
		const danish = localizeIn('da');

		expect(umbFormatDonutShare(50000 / 51233, danish)).to.equal('97,6 %');
		expect(umbFormatDonutShare(0.0004, danish)).to.equal('<0,1 %');
		expect(umbFormatDonutShare(0.9996, danish)).to.equal('>99,9 %');
	});
});
