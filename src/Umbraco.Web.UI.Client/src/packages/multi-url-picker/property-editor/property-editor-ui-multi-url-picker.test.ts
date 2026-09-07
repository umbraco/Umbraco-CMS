import { UmbPropertyEditorUIMultiUrlPickerElement } from './property-editor-ui-multi-url-picker.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_INTERACTION_MEMORY_SCOPE_CONTEXT,
	UmbInteractionMemoryContext,
} from '@umbraco-cms/backoffice/interaction-memory';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { UmbInputMultiUrlElement } from '../components/input-multi-url/index.js';

@customElement('test-multi-url-picker-memory-host')
class UmbTestMemoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	public readonly interactionMemoryContext = new UmbInteractionMemoryContext(this);
}

describe('UmbPropertyEditorUIMultiUrlPickerElement', () => {
	let element: UmbPropertyEditorUIMultiUrlPickerElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-multi-url-picker></umb-property-editor-ui-multi-url-picker>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIMultiUrlPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	// The link picker modal reaches the input's memory scope over context; from there the memories
	// travel up to this property editor as a property and an event, and end up in the app-root store
	// keyed by a hash of the configuration.
	describe('interaction memory', () => {
		const memory = {
			unique: 'UmbLinkPickerModal',
			memories: [{ unique: 'location', value: { unique: 'folder-1' } }],
		};
		const config = () => new UmbPropertyEditorConfigCollection([{ alias: 'hideAnchor', value: true }]);

		let host: UmbTestMemoryHostElement;

		const addPropertyEditor = async (options?: { withoutConfig: boolean }) => {
			const propertyEditor = document.createElement('umb-property-editor-ui-multi-url-picker');
			propertyEditor.config = options?.withoutConfig ? undefined : config();
			host.appendChild(propertyEditor);
			await propertyEditor.updateComplete;
			const input = propertyEditor.shadowRoot!.querySelector<UmbInputMultiUrlElement>('umb-input-multi-url')!;
			await input.updateComplete;
			const scope = await input.getContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT);
			return { propertyEditor, input, scope: scope!.memory };
		};

		const storedMemories = () =>
			new Promise<Array<{ unique: string; memories?: unknown }>>((resolve) => {
				host.interactionMemoryContext.memory.memories.subscribe((memories) => {
					if (memories.length > 0) resolve(memories);
				});
			});

		beforeEach(() => {
			host = new UmbTestMemoryHostElement();
			document.body.appendChild(host);
		});

		afterEach(() => {
			host.remove();
		});

		it('persists memories written to the input scope into the app-root store', async () => {
			const { scope } = await addPropertyEditor();
			scope.setMemory(memory);
			const stored = await storedMemories();
			expect(stored).to.have.lengthOf(1);
			expect(stored[0].unique.startsWith('UmbMultiUrlPickerPropertyEditorUi')).to.be.true;
			expect(stored[0].memories).to.deep.equal([memory]);
		});

		it('hands stored memories back to the input scope after the property editor is recreated', async () => {
			const first = await addPropertyEditor();
			first.scope.setMemory(memory);
			await storedMemories();

			first.propertyEditor.remove();

			const { scope } = await addPropertyEditor();
			const restored = await new Promise<unknown>((resolve) => {
				scope.memory(memory.unique).subscribe((value) => {
					if (value) resolve(value);
				});
			});
			expect(restored).to.deep.equal(memory);
		});

		it('persists memories for a property editor that has no configuration', async () => {
			const { scope } = await addPropertyEditor({ withoutConfig: true });
			scope.setMemory(memory);
			const stored = await storedMemories();
			expect(stored).to.have.lengthOf(1);
			expect(stored[0].memories).to.deep.equal([memory]);
		});
	});
});
