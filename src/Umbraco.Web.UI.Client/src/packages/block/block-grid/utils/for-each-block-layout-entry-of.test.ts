import { expect } from '@open-wc/testing';
import { forEachBlockLayoutEntryOf } from './index.js';
import type { UmbBlockGridLayoutModel } from '../types.js';

function makeLayout(
	contentKey: string,
	options: { settingsKey?: string; areas?: Array<{ key: string; items: Array<UmbBlockGridLayoutModel> }> } = {},
): UmbBlockGridLayoutModel {
	return {
		contentKey,
		settingsKey: options.settingsKey,
		columnSpan: 12,
		rowSpan: 1,
		areas: options.areas,
	} as UmbBlockGridLayoutModel;
}

describe('forEachBlockLayoutEntryOf', () => {
	it('never invokes the callback for the root entry itself', async () => {
		const root = makeLayout('root');
		const visited: Array<string> = [];

		await forEachBlockLayoutEntryOf(root, async (entry) => {
			visited.push(entry.contentKey);
		});

		expect(visited).to.deep.equal([]);
	});

	it('visits each descendant exactly once, at every nesting level', async () => {
		const grandchild = makeLayout('grandchild');
		const child1 = makeLayout('child1', {
			settingsKey: 'child1-settings',
			areas: [{ key: 'nested-area', items: [grandchild] }],
		});
		const child2 = makeLayout('child2');
		const root = makeLayout('root', {
			areas: [{ key: 'root-area', items: [child1, child2] }],
		});

		const visited: Array<string> = [];
		await forEachBlockLayoutEntryOf(root, async (entry) => {
			visited.push(entry.contentKey);
		});

		expect(visited).to.have.members(['child1', 'child2', 'grandchild']);
		expect(visited).to.have.lengthOf(3);
	});

	it("passes each descendant's own contentKey and settingsKey, not the root's", async () => {
		const child = makeLayout('child', { settingsKey: 'child-settings' });
		const root = makeLayout('root', {
			settingsKey: 'root-settings',
			areas: [{ key: 'root-area', items: [child] }],
		});

		const seen: Array<{ contentKey: string; settingsKey: string | undefined }> = [];
		await forEachBlockLayoutEntryOf(root, async (entry) => {
			seen.push({ contentKey: entry.contentKey, settingsKey: entry.settingsKey ?? undefined });
		});

		expect(seen).to.deep.equal([{ contentKey: 'child', settingsKey: 'child-settings' }]);
	});

	it('passes the correct parentUnique and areaKey for a nested entry', async () => {
		const grandchild = makeLayout('grandchild');
		const child = makeLayout('child', {
			areas: [{ key: 'inner-area', items: [grandchild] }],
		});
		const root = makeLayout('root', {
			areas: [{ key: 'outer-area', items: [child] }],
		});

		const calls: Array<{ contentKey: string; parentUnique: string; areaKey: string }> = [];
		await forEachBlockLayoutEntryOf(root, async (entry, parentUnique, areaKey) => {
			calls.push({ contentKey: entry.contentKey, parentUnique, areaKey });
		});

		expect(calls).to.deep.include({ contentKey: 'child', parentUnique: 'root', areaKey: 'outer-area' });
		expect(calls).to.deep.include({ contentKey: 'grandchild', parentUnique: 'child', areaKey: 'inner-area' });
	});

	it('does nothing when the entry has no areas', async () => {
		const root = makeLayout('root');
		let callCount = 0;

		await forEachBlockLayoutEntryOf(root, async () => {
			callCount++;
		});

		expect(callCount).to.equal(0);
	});
});
