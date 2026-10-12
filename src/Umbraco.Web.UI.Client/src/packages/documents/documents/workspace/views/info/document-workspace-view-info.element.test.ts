import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UmbTemplateItemStore } from '../../../../../templating/templates/repository/item/template-item.store.js';
import { UmbDocumentWorkspaceViewInfoElement } from './document-workspace-view-info.element.js';
import { aTimeout, expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ITEM_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbItemPickerModalData, UmbItemPickerModel } from '@umbraco-cms/backoffice/modal';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

// Template ids from the default mock data set: "Child" is nested under the "Test" layout.
const LAYOUT_TEMPLATE_ID = '9a84c0b3-03b4-4dd4-84ac-706740ac0f71';
const CHILD_TEMPLATE_ID = '9a84c0b3-03b4-4dd4-84ac-706740ac0f72';
const DOC_1_TEMPLATE_ID = '2bf464b6-3aca-4388-b043-4eb439cc2643';

/** Stands in for `UmbDocumentWorkspaceContext`, exposing only the state the info view reads and writes. */
class UmbTestDocumentWorkspaceContext {
	#host: HTMLElement;

	#ownerContentType = new UmbObjectState<
		{ name: string; icon: string; allowedTemplates: Array<{ id: string }> } | undefined
	>(undefined);
	readonly structure = { ownerContentType: this.#ownerContentType.asObservable() };

	#unique = new UmbStringState<string | undefined>('document-1');
	readonly unique = this.#unique.asObservable();

	#templateId = new UmbStringState<string | null>(null);
	readonly templateId = this.#templateId.asObservable();

	readonly setTemplateCalls: Array<string | null> = [];

	constructor(host: HTMLElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	// The context token only resolves providers whose entity type matches the document workspace.
	getEntityType() {
		return UMB_DOCUMENT_ENTITY_TYPE;
	}

	getContentTypeId() {
		return 'document-type-1';
	}

	setAllowedTemplates(ids: Array<string>) {
		this.#ownerContentType.setValue({
			name: 'Page',
			icon: 'icon-document',
			allowedTemplates: ids.map((id) => ({ id })),
		});
	}

	setCurrentTemplate(templateUnique: string | null) {
		this.#templateId.setValue(templateUnique);
	}

	setTemplate(templateUnique: string | null) {
		this.setTemplateCalls.push(templateUnique);
	}
}

@customElement('umb-test-document-workspace-view-info-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestDocumentWorkspaceViewInfoHostElement extends UmbElementMixin(HTMLElement) {}

// Stands in for the real section permission condition, so the view can render without the current-user stack.
class UmbTestSectionUserPermissionCondition extends UmbConditionBase<UmbConditionConfigBase> {
	constructor(host: UmbControllerHost, args: { config: UmbConditionConfigBase; onChange: () => void }) {
		super(host, args);
		this.permitted = false;
	}
}

describe('UmbDocumentWorkspaceViewInfoElement', () => {
	let host: UmbTestDocumentWorkspaceViewInfoHostElement;
	let element: UmbDocumentWorkspaceViewInfoElement;
	let context: UmbTestDocumentWorkspaceContext;
	let openedModals: Array<{ token: unknown; data: UmbItemPickerModalData }>;
	let pickedValue: UmbItemPickerModel | undefined;

	before(() => {
		umbExtensionsRegistry.register({
			type: 'condition',
			name: 'Test Section User Permission Condition',
			alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
			api: UmbTestSectionUserPermissionCondition,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS);
	});

	beforeEach(async () => {
		host = await fixture(
			html`<umb-test-document-workspace-view-info-host>
				<umb-document-workspace-view-info></umb-document-workspace-view-info>
			</umb-test-document-workspace-view-info-host>`,
		);
		element = host.querySelector('umb-document-workspace-view-info') as UmbDocumentWorkspaceViewInfoElement;

		openedModals = [];
		pickedValue = undefined;
		const modalManager = {
			getHostElement: () => host,
			open: (_host: unknown, token: unknown, args: { data: UmbItemPickerModalData }) => {
				openedModals.push({ token, data: args.data });
				return {
					onSubmit: () => (pickedValue ? Promise.resolve(pickedValue) : Promise.reject(new Error('Closed'))),
				};
			},
		};
		host.provideContext(UMB_MODAL_MANAGER_CONTEXT, modalManager as never);

		new UmbTemplateItemStore(host);

		context = new UmbTestDocumentWorkspaceContext(host);
		host.provideContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, context as never);
		await aTimeout(0);
	});

	async function openTemplatePicker() {
		await element.updateComplete;
		// Without a current template the placeholder button is shown; otherwise "Choose" is the first action on the template.
		const chooseButton = element.shadowRoot!.querySelector<HTMLElement>(
			'uui-button[look="placeholder"], uui-action-bar uui-button',
		);
		expect(chooseButton, 'template choose button').to.exist;
		chooseButton!.click();
		await waitUntil(() => openedModals.length > 0, 'template picker was not opened');
		return openedModals[0];
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDocumentWorkspaceViewInfoElement);
	});

	describe('template picker', () => {
		it('offers only the templates allowed by the document type, ignoring the template structure', async () => {
			context.setAllowedTemplates([DOC_1_TEMPLATE_ID, CHILD_TEMPLATE_ID]);

			const modal = await openTemplatePicker();
			const offered = modal.data.items.map((item) => item.value);

			expect(modal.token).to.equal(UMB_ITEM_PICKER_MODAL);
			expect(offered).to.have.members([CHILD_TEMPLATE_ID, DOC_1_TEMPLATE_ID]);
			expect(offered).to.not.include(LAYOUT_TEMPLATE_ID);
		});

		it('lists the allowed templates by name, with their alias and template icon', async () => {
			context.setAllowedTemplates([DOC_1_TEMPLATE_ID, CHILD_TEMPLATE_ID]);

			const modal = await openTemplatePicker();

			expect(modal.data.items).to.deep.equal([
				{ label: 'Child', value: CHILD_TEMPLATE_ID, icon: 'icon-document-html', description: 'Test' },
				{ label: 'Doc 1', value: DOC_1_TEMPLATE_ID, icon: 'icon-document-html', description: 'Doc1' },
			]);
		});

		it('marks the current template in its description', async () => {
			context.setAllowedTemplates([DOC_1_TEMPLATE_ID, CHILD_TEMPLATE_ID]);
			context.setCurrentTemplate(DOC_1_TEMPLATE_ID);

			const modal = await openTemplatePicker();
			const descriptions = modal.data.items.map((item) => item.description);

			expect(descriptions).to.deep.equal(['Test', `Doc1 (${element.localize.term('general_current')})`]);
		});

		it('sets the picked template on the document', async () => {
			context.setAllowedTemplates([DOC_1_TEMPLATE_ID, CHILD_TEMPLATE_ID]);
			pickedValue = { label: 'Doc 1', value: DOC_1_TEMPLATE_ID };

			await openTemplatePicker();
			await waitUntil(() => context.setTemplateCalls.length > 0, 'template was not set');

			expect(context.setTemplateCalls).to.deep.equal([DOC_1_TEMPLATE_ID]);
		});

		it('leaves the template unchanged when the picker is closed', async () => {
			context.setAllowedTemplates([DOC_1_TEMPLATE_ID]);

			await openTemplatePicker();
			await aTimeout(0);

			expect(context.setTemplateCalls).to.be.empty;
		});
	});
});
