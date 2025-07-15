import { expect } from '@open-wc/testing';
import { debounce } from './debounce.function.js';

describe('debounce', () => {
	it('should call the function only once after the timeout', async () => {
		let count = 0;
		const debounced = debounce(() => count++, 100);

		debounced();
		debounced();
		debounced();
		debounced();
		debounced();

		expect(count).to.equal(0);

		await new Promise((resolve) => setTimeout(resolve, 200));

		expect(count).to.equal(1);
	});

	it('should call the function with the latest arguments', async () => {
		let count = 0;
		const debounced = debounce((value: number) => (count = value), 100);

		debounced(1);
		debounced(2);
		debounced(3);
		debounced(4);
		debounced(5);

		expect(count).to.equal(0);

		await new Promise((resolve) => setTimeout(resolve, 200));

		expect(count).to.equal(5);
	});
});
