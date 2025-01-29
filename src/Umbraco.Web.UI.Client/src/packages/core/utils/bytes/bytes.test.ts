import { expect } from '@open-wc/testing';
import { formatBytes } from './bytes.function.js';

describe('bytes', () => {
	it('should format bytes as human-readable text', () => {
		expect(formatBytes(0)).to.equal('0 Bytes');
		expect(formatBytes(1024)).to.equal('1 KB');
		expect(formatBytes(1024 * 1024)).to.equal('1 MB');
		expect(formatBytes(1024 * 1024 * 1024)).to.equal('1 GB');
		expect(formatBytes(1024 * 1024 * 1024 * 1024)).to.equal('1 TB');
	});

	it('should format bytes as human-readable text with decimal places', () => {
		expect(formatBytes(1587.2, { decimals: 0 })).to.equal('2 KB');
		expect(formatBytes(1587.2, { decimals: 1 })).to.equal('1.6 KB');
	});

	it('should format bytes as human-readable text with different kilobytes', () => {
		expect(formatBytes(1000, { kilo: 1000 })).to.equal('1 KB');
		expect(formatBytes(1000 * 1000, { kilo: 1000 })).to.equal('1 MB');
		expect(formatBytes(1000 * 1000 * 1000, { kilo: 1000 })).to.equal('1 GB');
		expect(formatBytes(1000 * 1000 * 1000 * 1000, { kilo: 1000 })).to.equal('1 TB');
	});

	it('should format bytes as human-readable text with different culture', () => {
		expect(formatBytes(1587.2, { decimals: 1, culture: 'da-DK' })).to.equal('1,6 KB');
	});
});
