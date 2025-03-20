import type { UmbContentPickerDynamicRoot, UmbContentPickerDynamicRootQueryStep } from '../../types.js';
import {
	UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL,
	UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL,
} from '../modals/index.js';
import type { ManifestDynamicRootOrigin, ManifestDynamicRootQueryStep } from '../dynamic-root.extension.js';
import { html, css, customElement, property, ifDefined, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbDocumentTypeItemRepository } from '@umbraco-cms/backoffice/document-type';

const elementName = 'umb-input-content-picker-document-root';
@customElement(elementName)
export class UmbInputContentPickerDocumentRootElement extends UmbFormControlMixin<
	string | undefined,
	typeof UmbLitElement
>(UmbLitElement) {
	protected override getFormElement() {
		return undefined;
	}

	@state()
	private _originManifests: Array<ManifestDynamicRootOrigin> = [];

	@state()
	private _queryStepManifests: Array<ManifestDynamicRootQueryStep> = [];

	@property({ attribute: false })
	data?: UmbContentPickerDynamicRoot;

	#dynamicRootOrigin?: { label: string; icon: string; description?: string };

	#queryStepManifestsDocTypeNameMap: Record<string, string> = {};

	#documentTypeItemRepository: UmbDocumentTypeItemRepository = new UmbDocumentTypeItemRepository(this);

	#modalContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	#openModal?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});

		this.observe(
			umbExtensionsRegistry.byType('dynamicRootOrigin'),
			(originManifests: Array<ManifestDynamicRootOrigin>) => {
				this._originManifests = originManifests;
			},
		);

		this.observe(
			umbExtensionsRegistry.byType('dynamicRootQueryStep'),
			(queryStepManifests: Array<ManifestDynamicRootQueryStep>) => {
				this._queryStepManifests = queryStepManifests;
			},
		);
	}

	override async connectedCallback() {
		super.connectedCallback();

		this.#updateDynamicRootOrigin(this.data);
		this.#updateDynamicRootQuerySteps(this.data?.querySteps);
		await this.#populateQueryStepManifestsDocTypeNameMap();
	}

	#sorter = new UmbSorterController<UmbContentPickerDynamicRootQueryStep>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry.unique;
		},
		identifier: 'Umb.SorterIdentifier.InputDynamicRoot',
		itemSelector: 'uui-ref-node',
		containerSelector: '#query-steps',
		onChange: ({ model }) => {
			if (this.data?.querySteps) {
				const querySteps = model;
				this.#updateDynamicRootQuerySteps(querySteps);
				this.dispatchEvent(new UmbChangeEvent());
			}
		},
	});

	async #populateQueryStepManifestsDocTypeNameMap() {
		const docTypeKeys = this.data?.querySteps?.flatMap(item => item.anyOfDocTypeKeys).filter(item => item !== undefined) ?? [];
		if (docTypeKeys.length > 0) {
			const { data } = await this.#documentTypeItemRepository.requestItems(docTypeKeys);
			if (data) {
				for (let index = 0; index < data.length; index++) {
					const docTypeItem = data[index];
					this.#queryStepManifestsDocTypeNameMap[docTypeItem.unique] = docTypeItem.name;
				}
			}
		}

		this.requestUpdate();
	}

	#openDynamicRootOriginPicker() {
		this.#openModal = this.#modalContext?.open(this, UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL, {
			data: { items: this._originManifests },
		});
		this.#openModal?.onSubmit().then((data: UmbContentPickerDynamicRoot) => {
			const existingData = { ...this.data };
			existingData.originKey = undefined;
			this.data = { ...existingData, ...data };
			this.#updateDynamicRootOrigin(this.data);
			this.dispatchEvent(new UmbChangeEvent());
		});
	}

	#openDynamicRootQueryStepPicker() {
		this.#openModal = this.#modalContext?.open(this, UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL, {
			data: { items: this._queryStepManifests },
		});
		this.#openModal?.onSubmit().then(async (step) => {
			if (this.data) {
				const querySteps = [...(this.data.querySteps ?? []), step];
				this.#updateDynamicRootQuerySteps(querySteps);
				await this.#populateQueryStepManifestsDocTypeNameMap();
				this.dispatchEvent(new UmbChangeEvent());
			}
		});
	}

	#updateDynamicRootOrigin(data?: UmbContentPickerDynamicRoot) {
		if (!data) return;
		const origin = this._originManifests.find((item) => item.meta.originAlias === data.originAlias)?.meta;
		this.#dynamicRootOrigin = {
			label: origin?.label ?? data.originAlias,
			icon: origin?.icon ?? 'icon-wand',
			description: data.originKey,
		};
	}

	#updateDynamicRootQuerySteps(querySteps?: Array<UmbContentPickerDynamicRootQueryStep>) {
		if (!this.data) return;

		if (querySteps) {
			// NOTE: Ensure that the `unique` ID is populated for each query step. [LK]
			querySteps = querySteps.map((item) => (item.unique ? item : { ...item, unique: UmbId.new() }));
		}

		this.#sorter?.setModel(querySteps ?? []);
		this.data = { ...this.data, ...{ querySteps } };
	}

	#getQueryStepMeta(item: UmbContentPickerDynamicRootQueryStep): {
		unique: string;
		label: string;
		icon: string;
		description?: string;
		} {
		const step = this._queryStepManifests.find((step) => step.meta.queryStepAlias === item.alias)?.meta;

		const docTypeNames = item.anyOfDocTypeKeys?.map((item) => this.#queryStepManifestsDocTypeNameMap[item] ?? item).sort().join(', ') ?? '';
		const description = item.anyOfDocTypeKeys ? this.localize.term('dynamicRoot_queryStepTypes') + docTypeNames : undefined;

		return {
			unique: item.unique,
			label: step?.label ?? item.alias,
			icon: step?.icon ?? 'icon-lab',
			description,
		};
	}

	#removeDynamicRootQueryStep(item: UmbContentPickerDynamicRootQueryStep) {
		if (this.data?.querySteps) {
			const index = this.data.querySteps.indexOf(item);
			if (index !== -1) {
				const querySteps = [...this.data.querySteps];
				querySteps.splice(index, 1);
				this.#updateDynamicRootQuerySteps(querySteps);
				this.dispatchEvent(new UmbChangeEvent());
			}
		}
	}

	#clearDynamicRootQuery() {
		this.data = undefined;
		this.#dynamicRootOrigin = undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			${this.#renderAddOriginButton()}
			<uui-ref-list>${this.#renderOrigin()}</uui-ref-list>
			<uui-ref-list id="query-steps">${this.#renderQuerySteps()}</uui-ref-list>
			${this.#renderAddQueryStepButton()} ${this.#renderClearButton()}
		`;
	}

	#renderAddOriginButton() {
		if (this.data?.originAlias) return;
		return html`
			<uui-button
				class="add-button"
				@click=${this.#openDynamicRootOriginPicker}
				label=${this.localize.term('contentPicker_defineDynamicRoot')}
				look="placeholder"></uui-button>
		`;
	}

	#renderOrigin() {
		if (!this.#dynamicRootOrigin) return;
		return html`
			<uui-ref-node
				standalone
				name=${this.#dynamicRootOrigin.label}
				detail=${ifDefined(this.#dynamicRootOrigin.description)}>
				<umb-icon slot="icon" name=${ifDefined(this.#dynamicRootOrigin.icon)}></umb-icon>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${this.#openDynamicRootOriginPicker}
						label="${this.localize.term('general_edit')}"></uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderClearButton() {
		if (!this.#dynamicRootOrigin) return;
		return html`
			<uui-button @click=${this.#clearDynamicRootQuery}>${this.localize.term('buttons_clearSelection')}</uui-button>
		`;
	}

	#renderQuerySteps() {
		if (!this.data?.querySteps) return;
		return repeat(
			this.data.querySteps,
			(item) => item.unique,
			(item) => this.#renderQueryStep(item),
		);
	}

	#renderQueryStep(item: UmbContentPickerDynamicRootQueryStep) {
		if (!item.alias) return;
		const step = this.#getQueryStepMeta(item);
		return html`
			<uui-ref-node standalone id=${step.unique} name=${step.label} detail="${ifDefined(step.description)}">
				<umb-icon slot="icon" name=${step.icon}></umb-icon>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#removeDynamicRootQueryStep(item)}
						label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderAddQueryStepButton() {
		if (!this.#dynamicRootOrigin) return;
		return html` <uui-button
			class="add-button"
			@click=${this.#openDynamicRootQueryStepPicker}
			label=${this.localize.term('dynamicRoot_addQueryStep')}
			look="placeholder"></uui-button>`;
	}

	static override readonly styles = [
		css`
			.add-button {
				width: 100%;
			}

			uui-ref-node[drag-placeholder] {
				opacity: 0.2;
			}
		`,
	];
}

export { UmbInputContentPickerDocumentRootElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbInputContentPickerDocumentRootElement;
	}
}
