import { UmbPropertyEditorUiPickerMemoryContext } from './property-editor-ui-picker-memory.context.js';
import { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbInteractionMemoryContext } from '@umbraco-cms/backoffice/interaction-memory';

@customElement('test-picker-memory-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	public interactionMemoryContext: UmbInteractionMemoryContext;
	constructor() {
		super();
		this.interactionMemoryContext = new UmbInteractionMemoryContext(this);
	}
}

const config = new UmbPropertyEditorConfigCollection([{ alias: 'someAlias', value: 'someValue' }]);
const testMemory = { unique: 'UmbPickerModal:Test.Modal.Alias', value: { unique: 'folder-1' } };

describe('UmbPropertyEditorUiPickerMemoryContext', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbPropertyEditorUiPickerMemoryContext;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		context = new UmbPropertyEditorUiPickerMemoryContext(hostElement, { memoryUniquePrefix: 'TestPrefix' });
		context.setPropertyEditorConfig(config);
	});

	afterEach(() => {
		hostElement.remove();
	});

	it('has an interactionMemory manager', () => {
		expect(context).to.have.property('interactionMemory');
	});

	it('persists memories set on the scope into the property editor store, config-hash keyed', (done) => {
		hostElement.interactionMemoryContext.memory.memories.subscribe((allMemories) => {
			if (allMemories.length === 0) return;
			expect(allMemories).to.have.lengthOf(1);
			expect(allMemories[0].unique.startsWith('TestPrefixPropertyEditorUi')).to.be.true;
			expect(allMemories[0].memories).to.deep.equal([testMemory]);
			done();
		});

		context.interactionMemory.setMemory(testMemory);
	});

	it('restores memories into a new context after the property editor reopens', (done) => {
		// Seed via `context`, wait for the write to actually land in the shared store, then simulate
		// the property editor being destroyed and recreated (e.g. the RTE closing and reopening).
		// A fresh context sharing the same prefix and config should pick the memory back up.
		hostElement.interactionMemoryContext.memory.memories.subscribe((allMemories) => {
			if (allMemories.length === 0) return;

			context.destroy();

			const secondContext = new UmbPropertyEditorUiPickerMemoryContext(hostElement, {
				memoryUniquePrefix: 'TestPrefix',
			});
			secondContext.setPropertyEditorConfig(config);

			secondContext.interactionMemory.memory(testMemory.unique).subscribe((memory) => {
				if (!memory) return;
				expect(memory.value).to.deep.equal({ unique: 'folder-1' });
				done();
			});
		});

		context.interactionMemory.setMemory(testMemory);
	});

	it('deletes the underlying entry once the scope has no memories left', (done) => {
		let sawSeededValue = false;

		hostElement.interactionMemoryContext.memory.memories.subscribe((allMemories) => {
			if (!sawSeededValue && allMemories.length > 0) {
				sawSeededValue = true;
				context.interactionMemory.deleteMemory(testMemory.unique);
				return;
			}
			if (sawSeededValue && allMemories.length === 0) {
				done();
			}
		});

		context.interactionMemory.setMemory(testMemory);
	});

	it('does not keep re-writing once the scope and the store already agree', (done) => {
		let writeCount = 0;
		hostElement.interactionMemoryContext.memory.memories.subscribe((allMemories) => {
			if (allMemories.length > 0) writeCount++;
		});

		context.interactionMemory.setMemory(testMemory);

		// Setting the exact same memory again should short-circuit via jsonStringComparison and not
		// trigger a second write to the underlying store.
		setTimeout(() => {
			context.interactionMemory.setMemory({ ...testMemory });
			setTimeout(() => {
				expect(writeCount).to.equal(1);
				done();
			}, 10);
		}, 10);
	});
});
