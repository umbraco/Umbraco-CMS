import { html, css, customElement, property, ifDefined, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import {
	UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL,
	UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL,
} from '@umbraco-cms/backoffice/dynamic-root';
import type {
	ManifestDynamicRootOrigin,
	ManifestDynamicRootQueryStep,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import type { UmbTreePickerDynamicRoot, UmbTreePickerDynamicRootQueryStep } from '@umbraco-cms/backoffice/components';

@customElement('umb-input-document-picker-root')
export class UmbInputDocumentPickerRootElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@state()
	private _originManifests: Array<ManifestDynamicRootOrigin> = [];

	@state()
	private _queryStepManifests: Array<ManifestDynamicRootQueryStep> = [];

	@property({ attribute: false })
	data?: UmbTreePickerDynamicRoot | undefined;

	#dynamicRootOrigin?: { label: string; icon: string; description?: string };

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

	connectedCallback(): void {
		super.connectedCallback();

		this.#updateDynamicRootOrigin(this.data);
		this.#updateDynamicRootQuerySteps(this.data?.querySteps);
	}

	#sorter = new UmbSorterController<UmbTreePickerDynamicRootQueryStep>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry.unique;
		},
		identifier: 'Umb.SorterIdentifier.InputDocumentPickerRoot',
		itemSelector: 'uui-ref-node',
		containerSelector: '#query-steps',
		onChange: ({ model }) => {
			if (this.data && this.data.querySteps) {
				const querySteps = model;
				this.#updateDynamicRootQuerySteps(querySteps);
				this.dispatchEvent(new UmbChangeEvent());
			}
		},
	});

	#openDynamicRootOriginPicker() {
		this.#openModal = this.#modalContext?.open(UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL, {});
		this.#openModal?.onSubmit().then((data: UmbTreePickerDynamicRoot) => {
			const existingData = { ...this.data };
			existingData.originKey = undefined;
			this.data = { ...existingData, ...data };
			this.#updateDynamicRootOrigin(this.data);
			this.dispatchEvent(new UmbChangeEvent());
		});
	}

	#openDynamicRootQueryStepPicker() {
		this.#openModal = this.#modalContext?.open(UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL, {});
		this.#openModal?.onSubmit().then((step) => {
			if (this.data) {
				const querySteps = [...(this.data.querySteps ?? []), step];
				this.#updateDynamicRootQuerySteps(querySteps);
				this.dispatchEvent(new UmbChangeEvent());
			}
		});
	}

	#updateDynamicRootOrigin(data?: UmbTreePickerDynamicRoot) {
		if (!data) return;
		const origin = this._originManifests.find((item) => item.meta.originAlias === data.originAlias)?.meta;
		this.#dynamicRootOrigin = {
			label: origin?.label ?? data.originAlias,
			icon: origin?.icon ?? 'icon-wand',
			description: data.originKey,
		};
	}

	#updateDynamicRootQuerySteps(querySteps?: Array<UmbTreePickerDynamicRootQueryStep>) {
		if (!this.data) return;

		if (querySteps) {
			// NOTE: Ensure that the `unique` ID is populated for each query step. [LK]
			querySteps = querySteps.map((item) => (item.unique ? item : { ...item, unique: UmbId.new() }));
		}

		this.#sorter?.setModel(querySteps ?? []);
		this.data = { ...this.data, ...{ querySteps } };
	}

	#getQueryStepMeta(item: UmbTreePickerDynamicRootQueryStep): {
		unique: string;
		label: string;
		icon: string;
		description?: string;
	} {
		const step = this._queryStepManifests.find((step) => step.meta.queryStepAlias === item.alias)?.meta;
		const docTypes = item.anyOfDocTypeKeys?.join(', ');
		const description = docTypes ? this.localize.term('dynamicRoot_queryStepTypes') + docTypes : undefined;

		return {
			unique: item.unique,
			label: step?.label ?? item.alias,
			icon: step?.icon ?? 'icon-lab',
			description,
		};
	}

	#removeDynamicRootQueryStep(item: UmbTreePickerDynamicRootQueryStep) {
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

	render() {
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
				border
				standalone
				name=${this.#dynamicRootOrigin.label}
				detail=${ifDefined(this.#dynamicRootOrigin.description)}>
				<uui-icon slot="icon" name=${ifDefined(this.#dynamicRootOrigin.icon)}></uui-icon>
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

	#renderQueryStep(item: UmbTreePickerDynamicRootQueryStep) {
		if (!item.alias) return;
		const step = this.#getQueryStepMeta(item);
		return html`
			<uui-ref-node border standalone id=${step.unique} name=${step.label} detail="${ifDefined(step.description)}">
				<uui-icon slot="icon" name=${step.icon}></uui-icon>
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

	static styles = [
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

export default UmbInputDocumentPickerRootElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-picker-root': UmbInputDocumentPickerRootElement;
	}
}
