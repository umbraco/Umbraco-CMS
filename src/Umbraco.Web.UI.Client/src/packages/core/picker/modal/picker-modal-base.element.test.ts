import { UmbPickerModalBaseElement } from './picker-modal-base.element.js';
import { UMB_PICKER_INTERACTION_MEMORY_CONTEXT } from './picker-interaction-memory.context.token.js';
import { UmbPickerContext } from '../picker.context.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

@customElement('test-picker-modal-scope-host')
class UmbTestScopeHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	public readonly scope: UmbInteractionMemoryManager;
	constructor() {
		super();
		this.scope = new UmbInteractionMemoryManager(this);
		this.scope.provideContext(UMB_PICKER_INTERACTION_MEMORY_CONTEXT, this.scope);
	}
}

@customElement('umb-test-picker-modal')
class UmbTestPickerModalElement extends UmbPickerModalBaseElement {
	protected override _pickerContext = new UmbPickerContext(this);
	public get pickerContext() {
		return this._pickerContext;
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
