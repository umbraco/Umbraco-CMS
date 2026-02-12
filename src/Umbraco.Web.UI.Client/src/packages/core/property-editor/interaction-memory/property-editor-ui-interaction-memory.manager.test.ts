import { UmbPropertyEditorUiInteractionMemoryManager } from './property-editor-ui-interaction-memory.manager.js';
import { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbInteractionMemoryContext } from '@umbraco-cms/backoffice/interaction-memory';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbInteractionMemoryContext(this);
	}
}

describe('UmbPropertyEditorUiInteractionMemoryManager', () => {
	let manager: UmbPropertyEditorUiInteractionMemoryManager;
	let childMemories = [
		{ unique: '1', value: 'Value 1' },
		{ unique: '2', value: 'Value 2' },
	];

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

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
	});
});
