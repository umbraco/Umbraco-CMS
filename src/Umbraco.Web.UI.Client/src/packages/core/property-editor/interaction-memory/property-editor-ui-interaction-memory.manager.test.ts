import { UmbPropertyEditorUiInteractionMemoryManager } from './property-editor-ui-interaction-memory.manager.js';
import { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbInteractionMemoryContext } from '@umbraco-cms/backoffice/interaction-memory';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	public interactionMemoryContext: UmbInteractionMemoryContext;
	constructor() {
		super();
		this.interactionMemoryContext = new UmbInteractionMemoryContext(this);
	}
}

describe('UmbPropertyEditorUiInteractionMemoryManager', () => {
	let manager: UmbPropertyEditorUiInteractionMemoryManager;
	let hostElement: UmbTestControllerHostElement;
	let interactionMemoryContext: UmbInteractionMemoryContext;
	let childMemories = [
		{ unique: '1', value: 'Value 1' },
		{ unique: '2', value: 'Value 2' },
	];

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		interactionMemoryContext = hostElement.interactionMemoryContext;

		manager = new UmbPropertyEditorUiInteractionMemoryManager(hostElement, {
			memoryUniquePrefix: 'TestPrefix',
		});

		// A random config to generate a hash code from
		const config = new UmbPropertyEditorConfigCollection([
			{
				alias: 'someAlias',
				value: 'someValue',
			},
		]);

		manager.setPropertyEditorConfig(config);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a memoriesForPropertyEditor property', () => {
				expect(manager).to.have.property('memoriesForPropertyEditor').to.be.an.instanceOf(Observable);
			});
		});

		describe('methods', () => {
			it('has a setPropertyEditorConfig method', () => {
				expect(manager).to.have.property('setPropertyEditorConfig').that.is.a('function');
			});

			it('has a saveMemoriesForPropertyEditor method', () => {
				expect(manager).to.have.property('saveMemoriesForPropertyEditor').that.is.a('function');
			});

			it('has a deleteMemoriesForPropertyEditor method', () => {
				expect(manager).to.have.property('deleteMemoriesForPropertyEditor').that.is.a('function');
			});
		});

		describe('saveMemoriesForPropertyEditor', () => {
			it('creates a property editor memory based on the provided data', (done) => {
				manager.memoriesForPropertyEditor.subscribe((memories) => {
					if (memories.length > 0) {
						expect(memories).to.have.lengthOf(2);
						expect(memories).to.deep.equal(childMemories);
						done();
					}
				});

				manager.saveMemoriesForPropertyEditor(childMemories);
			});

			it('updates the property editor memory based on the provided data', (done) => {
				const updatedChildMemories = [
					{ unique: '1', value: 'Updated Value 1' },
					{ unique: '2', value: 'Updated Value 2' },
					{ unique: '3', value: 'New Value 3' },
				];

				// We start at -1 because the first call is the initial empty array
				let callCount = -1;
				manager.memoriesForPropertyEditor.subscribe((memories) => {
					callCount++;
					if (callCount === 1) {
						// First call, after initial save
						expect(memories).to.have.lengthOf(2);
						expect(memories).to.deep.equal(childMemories);
					} else if (callCount === 2) {
						// Second call, after update
						expect(memories).to.have.lengthOf(3);
						expect(memories).to.deep.equal(updatedChildMemories);
						done();
					}
				});

				manager.saveMemoriesForPropertyEditor(childMemories);
				manager.saveMemoriesForPropertyEditor(updatedChildMemories);
			});
		});

		describe('observes the interactionMemoryContext', () => {
			it('reflects memory updates made directly on the context', async () => {
				// Seed via the manager so the context owns an entry whose unique we can discover.
				await manager.saveMemoriesForPropertyEditor(childMemories);

				const storedMemories = interactionMemoryContext.memory.getAllMemories();
				expect(storedMemories).to.have.lengthOf(1);
				const memoryUnique = storedMemories[0].unique;

				const externalMemories = [
					{ unique: 'external-1', value: 'External Value 1' },
					{ unique: 'external-2', value: 'External Value 2' },
					{ unique: 'external-3', value: 'External Value 3' },
				];

				const updatePropagated = new Promise<void>((resolve) => {
					manager.memoriesForPropertyEditor.subscribe((memories) => {
						if (memories.length === externalMemories.length) {
							expect(memories).to.deep.equal(externalMemories);
							resolve();
						}
					});
				});

				interactionMemoryContext.memory.setMemory({
					unique: memoryUnique,
					memories: externalMemories,
				});

				await updatePropagated;
			});

			it('clears its memories when the context deletes the entry', async () => {
				await manager.saveMemoriesForPropertyEditor(childMemories);

				const storedMemories = interactionMemoryContext.memory.getAllMemories();
				expect(storedMemories).to.have.lengthOf(1);
				const memoryUnique = storedMemories[0].unique;

				const deletionPropagated = new Promise<void>((resolve) => {
					let sawSeededValue = false;
					manager.memoriesForPropertyEditor.subscribe((memories) => {
						if (!sawSeededValue && memories.length === childMemories.length) {
							sawSeededValue = true;
							return;
						}
						if (sawSeededValue && memories.length === 0) {
							resolve();
						}
					});
				});

				interactionMemoryContext.memory.deleteMemory(memoryUnique);

				await deletionPropagated;
			});
		});

		describe('deleteMemoriesForPropertyEditor', () => {
			it('deletes all memories for this property editor', (done) => {
				// We start at -1 because the first call is the initial empty array
				let callCount = -1;
				manager.memoriesForPropertyEditor.subscribe((memories) => {
					callCount++;
					if (callCount === 1) {
						// First call, after initial save
						expect(memories).to.have.lengthOf(2);
						expect(memories).to.deep.equal(childMemories);
					} else if (callCount === 2) {
						// Second call, after delete
						expect(memories).to.have.lengthOf(0);
						expect(memories).to.deep.equal([]);
						done();
					}
				});

				manager.saveMemoriesForPropertyEditor(childMemories);
				manager.deleteMemoriesForPropertyEditor();
			});
		});

		// This manager also acts as the interaction-memory scope a picker modal is handed (see
		// UMB_PICKER_INTERACTION_MEMORY_CONTEXT), so it needs the per-key API too, on top of the
		// list-oriented one above.
		describe('acts as an interaction-memory scope (per-key API)', () => {
			const testMemory = { unique: 'UmbPickerModal:Test.Modal.Alias', value: { unique: 'folder-1' } };

			it('has the inherited per-key API', () => {
				expect(manager).to.have.property('memory').that.is.a('function');
				expect(manager).to.have.property('setMemory').that.is.a('function');
				expect(manager).to.have.property('deleteMemory').that.is.a('function');
			});

			it('persists a memory set via setMemory into the property editor store, config-hash keyed', (done) => {
				interactionMemoryContext.memory.memories.subscribe((allMemories) => {
					if (allMemories.length === 0) return;
					expect(allMemories).to.have.lengthOf(1);
					expect(allMemories[0].unique.startsWith('TestPrefixPropertyEditorUi')).to.be.true;
					expect(allMemories[0].memories).to.deep.equal([testMemory]);
					done();
				});

				manager.setMemory(testMemory);
			});

			it('restores memories into a new manager after the property editor reopens', (done) => {
				// Seed via `manager`, wait for the write to actually land in the shared store, then simulate
				// the property editor being destroyed and recreated (e.g. the RTE closing and reopening) —
				// note this reuses the SAME `hostElement`, since the app-root store the manager persists to
				// is a page-wide singleton that outlives any one property editor instance. A fresh manager
				// sharing the same prefix and config should pick the memory back up.
				interactionMemoryContext.memory.memories.subscribe((allMemories) => {
					if (allMemories.length === 0) return;

					manager.destroy();

					const secondManager = new UmbPropertyEditorUiInteractionMemoryManager(hostElement, {
						memoryUniquePrefix: 'TestPrefix',
					});
					secondManager.setPropertyEditorConfig(
						new UmbPropertyEditorConfigCollection([{ alias: 'someAlias', value: 'someValue' }]),
					);

					secondManager.memory(testMemory.unique).subscribe((memory) => {
						if (!memory) return;
						expect(memory.value).to.deep.equal({ unique: 'folder-1' });
						done();
					});
				});

				manager.setMemory(testMemory);
			});

			it('deletes the underlying entry once the last memory is removed', (done) => {
				let sawSeededValue = false;

				interactionMemoryContext.memory.memories.subscribe((allMemories) => {
					if (!sawSeededValue && allMemories.length > 0) {
						sawSeededValue = true;
						manager.deleteMemory(testMemory.unique);
						return;
					}
					if (sawSeededValue && allMemories.length === 0) {
						done();
					}
				});

				manager.setMemory(testMemory);
			});

			it('does not keep re-writing once the manager and the store already agree', (done) => {
				let writeCount = 0;
				interactionMemoryContext.memory.memories.subscribe((allMemories) => {
					if (allMemories.length > 0) writeCount++;
				});

				manager.setMemory(testMemory);

				// Setting the exact same memory again should short-circuit via the state's own dedup and
				// not trigger a second write to the underlying store — no reentrancy guard needed here.
				setTimeout(() => {
					manager.setMemory({ ...testMemory });
					setTimeout(() => {
						expect(writeCount).to.equal(1);
						done();
					}, 10);
				}, 10);
			});
		});
	});
});
