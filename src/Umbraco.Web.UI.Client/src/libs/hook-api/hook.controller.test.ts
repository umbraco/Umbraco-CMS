import { UmbHookController } from './hook.controller.js';
import { expect } from '@open-wc/testing';

describe('UmbHookController', () => {
	type TestData = { value: number; log?: Array<string> };

	let hook: UmbHookController<TestData>;

	beforeEach(() => {
		hook = new UmbHookController<TestData>();
	});

	it('returns initial data when no hooks are registered', async () => {
		const data: TestData = { value: 1 };
		const result = await hook.execute(data);
		expect(result).to.equal(data);
	});

	it('single hook transforms data', async () => {
		hook.add((data) => ({ ...data, value: data.value + 10 }));
		const result = await hook.execute({ value: 1 });
		expect(result.value).to.equal(11);
	});

	it('executes hooks in ascending weight order', async () => {
		const log: Array<number> = [];

		hook.add((data) => {
			log.push(20);
			return data;
		}, 20);

		hook.add((data) => {
			log.push(10);
			return data;
		}, 10);

		hook.add((data) => {
			log.push(30);
			return data;
		}, 30);

		await hook.execute({ value: 0 });
		expect(log).to.deep.equal([10, 20, 30]);
	});

	it('maintains insertion order for same weight', async () => {
		const log: Array<string> = [];

		hook.add((data) => {
			log.push('first');
			return data;
		}, 0);

		hook.add((data) => {
			log.push('second');
			return data;
		}, 0);

		hook.add((data) => {
			log.push('third');
			return data;
		}, 0);

		await hook.execute({ value: 0 });
		expect(log).to.deep.equal(['first', 'second', 'third']);
	});

	it('uses default weight of 0', async () => {
		const log: Array<string> = [];

		hook.add((data) => {
			log.push('default');
			return data;
		});

		hook.add((data) => {
			log.push('weight-1');
			return data;
		}, 1);

		hook.add((data) => {
			log.push('weight-neg1');
			return data;
		}, -1);

		await hook.execute({ value: 0 });
		expect(log).to.deep.equal(['weight-neg1', 'default', 'weight-1']);
	});

	it('awaits async hooks', async () => {
		hook.add(async (data) => {
			return new Promise<TestData>((resolve) => {
				setTimeout(() => resolve({ ...data, value: data.value + 5 }), 10);
			});
		});

		hook.add((data) => ({ ...data, value: data.value * 2 }));

		const result = await hook.execute({ value: 1 });
		expect(result.value).to.equal(12);
	});

	it('flows mutated data through the chain', async () => {
		hook.add(
			(data) => {
				data.value = 42;
				return data;
			},
			0,
		);

		hook.add(
			(data) => {
				data.log = [`received:${data.value}`];
				return data;
			},
			1,
		);

		const result = await hook.execute({ value: 0 });
		expect(result.value).to.equal(42);
		expect(result.log).to.deep.equal(['received:42']);
	});

	it('short-circuits when a hook throws', async () => {
		let thirdCalled = false;

		hook.add((data) => ({ ...data, value: 1 }), 0);

		hook.add(() => {
			throw new Error('cancelled');
		}, 1);

		hook.add((data) => {
			thirdCalled = true;
			return data;
		}, 2);

		try {
			await hook.execute({ value: 0 });
			expect.fail('should have thrown');
		} catch (e) {
			expect((e as Error).message).to.equal('cancelled');
		}

		expect(thirdCalled).to.be.false;
	});

	it('short-circuits on async rejection', async () => {
		let thirdCalled = false;

		hook.add((data) => ({ ...data, value: 1 }), 0);

		hook.add(async () => {
			return Promise.reject(new Error('async cancel'));
		}, 1);

		hook.add((data) => {
			thirdCalled = true;
			return data;
		}, 2);

		try {
			await hook.execute({ value: 0 });
			expect.fail('should have thrown');
		} catch (e) {
			expect((e as Error).message).to.equal('async cancel');
		}

		expect(thirdCalled).to.be.false;
	});

	it('removes a hook by method reference', async () => {
		const addTen = (data: TestData): TestData => ({ ...data, value: data.value + 10 });
		const addFive = (data: TestData): TestData => ({ ...data, value: data.value + 5 });

		hook.add(addTen);
		hook.add(addFive);

		hook.remove(addTen);

		const result = await hook.execute({ value: 0 });
		expect(result.value).to.equal(5);
	});

	it('destroy clears all hooks', async () => {
		hook.add((data) => ({ ...data, value: 999 }));
		hook.destroy();

		const result = await hook.execute({ value: 1 });
		expect(result.value).to.equal(1);
	});
});
