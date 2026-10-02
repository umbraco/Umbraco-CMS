import { UmbParallelHookController } from './parallel-hook.controller.js';
import { expect } from '@open-wc/testing';

const wait = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

describe('UmbParallelHookController', () => {
	let hook: UmbParallelHookController<{ name: string }>;

	beforeEach(() => {
		hook = new UmbParallelHookController<{ name: string }>();
	});

	it('resolves when no methods are added', async () => {
		await hook.execute({ name: 'test' });
	});

	it('passes the meta to every method', async () => {
		const received: Array<string> = [];
		hook.add((meta) => {
			received.push(meta.name);
		});
		hook.add((meta) => {
			received.push(meta.name);
		});

		await hook.execute({ name: 'test' });

		expect(received).to.deep.equal(['test', 'test']);
	});

	it('starts all methods before any of them has finished', async () => {
		const log: Array<string> = [];
		hook.add(async () => {
			log.push('a:start');
			await wait(20);
			log.push('a:end');
		});
		hook.add(async () => {
			log.push('b:start');
			await wait(10);
			log.push('b:end');
		});

		await hook.execute({ name: 'test' });

		expect(log).to.deep.equal(['a:start', 'b:start', 'b:end', 'a:end']);
	});

	it('takes as long as the slowest method, not the sum of them', async () => {
		hook.add(() => wait(100));
		hook.add(() => wait(100));
		hook.add(() => wait(100));

		const start = performance.now();
		await hook.execute({ name: 'test' });
		const elapsed = performance.now() - start;

		expect(elapsed).to.be.lessThan(250);
	});

	it('waits for the slowest method', async () => {
		let finished = false;
		hook.add(async () => {
			await wait(50);
			finished = true;
		});
		hook.add(() => {});

		await hook.execute({ name: 'test' });

		expect(finished).to.be.true;
	});

	describe('methods added while executing', () => {
		it('starts a method added by a running method, and waits for it', async () => {
			const log: Array<string> = [];
			hook.add(() => {
				hook.add(async () => {
					await wait(30);
					log.push('added');
				});
				log.push('adder');
			});

			await hook.execute({ name: 'test' });

			expect(log).to.deep.equal(['adder', 'added']);
		});

		it('starts a method added while another method is waiting, and waits for it', async () => {
			const log: Array<string> = [];
			hook.add(async () => {
				await wait(20);
				hook.add(async () => {
					await wait(20);
					log.push('added');
				});
			});
			hook.add(async () => {
				await wait(60);
				log.push('slow');
			});

			await hook.execute({ name: 'test' });

			expect(log).to.deep.equal(['added', 'slow']);
		});

		it('does not run a method added after the execution has finished', async () => {
			let calls = 0;
			await hook.execute({ name: 'test' });

			hook.add(() => {
				calls++;
			});
			await wait(20);

			expect(calls).to.equal(0);
		});

		it('runs a method once, even when several executions overlap', async () => {
			let calls = 0;
			hook.add(async () => {
				await wait(10);
				calls++;
			});

			await Promise.all([hook.execute({ name: 'one' }), hook.execute({ name: 'two' })]);

			expect(calls).to.equal(2);
		});
	});

	describe('errors', () => {
		it('lets every method finish before it throws the first failure', async () => {
			let slowFinished = false;
			hook.add(() => {
				throw new Error('first failure');
			});
			hook.add(async () => {
				await wait(30);
				slowFinished = true;
			});

			let error: unknown;
			await hook.execute({ name: 'test' }).catch((e) => (error = e));

			expect((error as Error).message).to.equal('first failure');
			expect(slowFinished).to.be.true;
		});

		it('throws for an async rejection', async () => {
			hook.add(async () => {
				throw new Error('async failure');
			});

			let error: unknown;
			await hook.execute({ name: 'test' }).catch((e) => (error = e));

			expect((error as Error).message).to.equal('async failure');
		});
	});

	it('does not run a method that has been removed', async () => {
		let calls = 0;
		const method = () => {
			calls++;
		};
		hook.add(method);
		hook.remove(method);

		await hook.execute({ name: 'test' });

		expect(calls).to.equal(0);
	});

	it('destroy clears all methods', async () => {
		let calls = 0;
		hook.add(() => {
			calls++;
		});
		hook.destroy();

		await hook.execute({ name: 'test' });

		expect(calls).to.equal(0);
	});
});
