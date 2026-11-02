import { UmbPickerModalBaseElement } from './picker-modal-base.element.js';
import { UmbPickerContext } from '../picker.context.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbInteractionMemoryManager,
	UmbInteractionMemoryScopeContext,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

@customElement('test-picker-modal-scope-host')
class UmbTestScopeHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	public readonly scope: UmbInteractionMemoryManager;
	constructor() {
		super();
		this.scope = new UmbInteractionMemoryManager(this);
		new UmbInteractionMemoryScopeContext(this, this.scope);
	}
}

@customElement('umb-test-picker-modal')
class UmbTestPickerModalElement extends UmbPickerModalBaseElement {
	protected override _pickerContext = new UmbPickerContext(this);
	public get pickerContext() {
		return this._pickerContext;
	}
}

// Mirrors how a real picker modal restores state: a synchronous `getMemory()` inside `firstUpdated`
// (see media-picker-modal.element.ts). The seed has to have landed before that runs.
@customElement('umb-test-picker-modal-timing')
class UmbTestPickerModalTimingElement extends UmbPickerModalBaseElement {
	protected override _pickerContext = new UmbPickerContext(this);
	public memoryAtFirstUpdated?: UmbInteractionMemoryModel;

	protected override firstUpdated(changedProperties: PropertyValues) {
		super.firstUpdated(changedProperties);
		this.memoryAtFirstUpdated = this._pickerContext.interactionMemory.getMemory('location');
	}
}

const testManifest: ManifestModal = { type: 'modal', alias: 'Test.Modal.Alias', name: 'Test Modal' };

describe('UmbPickerModalBaseElement', () => {
	let scopeHost: UmbTestScopeHostElement;

	beforeEach(() => {
		scopeHost = new UmbTestScopeHostElement();
		document.body.appendChild(scopeHost);
	});

	afterEach(() => {
		scopeHost.remove();
	});

	it('bridges its picker memories to the scope context, keyed by the modal manifest alias', (done) => {
		const element = new UmbTestPickerModalElement();
		element.manifest = testManifest;
		scopeHost.appendChild(element);

		scopeHost.scope.memory('UmbPickerModal:Test.Modal.Alias').subscribe((memory) => {
			if (!memory) return;
			expect(memory.memories).to.deep.equal([{ unique: 'location', value: { unique: 'folder-1' } }]);
			done();
		});

		element.pickerContext.interactionMemory.setMemory({ unique: 'location', value: { unique: 'folder-1' } });
	});

	it('restores memories from the scope context once connected', (done) => {
		scopeHost.scope.setMemory({
			unique: 'UmbPickerModal:Test.Modal.Alias',
			memories: [{ unique: 'location', value: { unique: 'folder-2' } }],
		});

		const element = new UmbTestPickerModalElement();
		element.manifest = testManifest;

		element.pickerContext.interactionMemory.memory('location').subscribe((memory) => {
			if (!memory) return;
			expect(memory.value).to.deep.equal({ unique: 'folder-2' });
			done();
		});

		scopeHost.appendChild(element);
	});

	it('keeps its picker memories when its entry disappears from the scope', async () => {
		const element = new UmbTestPickerModalElement();
		element.manifest = testManifest;
		scopeHost.appendChild(element);

		element.pickerContext.interactionMemory.setMemory({ unique: 'location', value: { unique: 'folder-5' } });
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Simulates the scope being emptied from above while this modal is still open.
		scopeHost.scope.deleteMemory('UmbPickerModal:Test.Modal.Alias');
		await new Promise((resolve) => setTimeout(resolve, 0));

		expect(element.pickerContext.interactionMemory.getMemory('location')?.value).to.deep.equal({
			unique: 'folder-5',
		});
	});

	it('has restored its memories from the scope by the time firstUpdated runs', async () => {
		scopeHost.scope.setMemory({
			unique: 'UmbPickerModal:Test.Modal.Alias',
			memories: [{ unique: 'location', value: { unique: 'folder-6' } }],
		});

		const element = new UmbTestPickerModalTimingElement();
		element.manifest = testManifest;
		scopeHost.appendChild(element);
		await element.updateComplete;

		expect(element.memoryAtFirstUpdated?.value).to.deep.equal({ unique: 'folder-6' });
	});

	it('does not throw when no scope context is available', () => {
		const element = new UmbTestPickerModalElement();
		element.manifest = testManifest;
		document.body.appendChild(element);

		expect(() =>
			element.pickerContext.interactionMemory.setMemory({ unique: 'location', value: { unique: 'folder-3' } }),
		).to.not.throw();

		element.remove();
	});

	it('keys memories separately per modal alias, leaving other aliases untouched', (done) => {
		scopeHost.scope.setMemory({
			unique: 'UmbPickerModal:Other.Modal.Alias',
			memories: [{ unique: 'location', value: { unique: 'other-folder' } }],
		});

		const element = new UmbTestPickerModalElement();
		element.manifest = testManifest;
		scopeHost.appendChild(element);

		scopeHost.scope.memory('UmbPickerModal:Test.Modal.Alias').subscribe((memory) => {
			if (!memory) return;
			expect(memory.memories).to.deep.equal([{ unique: 'location', value: { unique: 'folder-4' } }]);
			expect(scopeHost.scope.getMemory('UmbPickerModal:Other.Modal.Alias')?.memories).to.deep.equal([
				{ unique: 'location', value: { unique: 'other-folder' } },
			]);
			done();
		});

		element.pickerContext.interactionMemory.setMemory({ unique: 'location', value: { unique: 'folder-4' } });
	});
});
