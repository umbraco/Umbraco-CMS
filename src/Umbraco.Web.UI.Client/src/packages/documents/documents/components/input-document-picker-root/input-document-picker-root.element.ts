import {
	UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL,
	UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL,
} from '@umbraco-cms/backoffice/dynamic-root';
import { html, css, customElement, property, ifDefined, map, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbTreePickerDynamicRoot, UmbTreePickerDynamicRootQueryStep } from '@umbraco-cms/backoffice/components';
import type { UmbModalContext, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import {
	type ManifestDynamicRootOrigin,
	type ManifestDynamicRootQueryStep,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-input-document-picker-root')
export class UmbInputDocumentPickerRootElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@state()
	private _origins: Array<ManifestDynamicRootOrigin> = [];

	@state()
	private _querySteps: Array<ManifestDynamicRootQueryStep> = [];

	@property({ attribute: false })
	data?: UmbTreePickerDynamicRoot | undefined;

	#dynamicRootOrigin?: { label: string; icon: string; description?: string };

	#modalContext?: UmbModalManagerContext;

	#openModal?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});

		this.observe(umbExtensionsRegistry.byType('dynamicRootOrigin'), (origins: Array<ManifestDynamicRootOrigin>) => {
			this._origins = origins;
		});

		this.observe(
			umbExtensionsRegistry.byType('dynamicRootQueryStep'),
			(querySteps: Array<ManifestDynamicRootQueryStep>) => {
				this._querySteps = querySteps;
			},
		);
	}

	connectedCallback(): void {
		super.connectedCallback();

		this.#updateDynamicRootOrigin(this.data);
		this.#updateDynamicRootQuerySteps(this.data?.querySteps);
	}

	#sorter = new UmbSorterController(this, {
		compareElementToModel: (element: HTMLElement, model: string) => {
			return element.getAttribute('data-idx') === model;
		},
		querySelectModelToElement: () => null,
		identifier: 'Umb.SorterIdentifier.InputDocumentPickerRoot',
		itemSelector: 'uui-ref-node',
		containerSelector: '#query-steps',
		onChange: ({ model }) => {
			if (this.data && this.data.querySteps) {
				const steps = [...this.data.querySteps];
				const querySteps = model.map((index) => steps[parseInt(index)]);
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
		const origin = this._origins.find((item) => item.meta.originAlias === data.originAlias)?.meta;
		this.#dynamicRootOrigin = {
			label: origin?.label ?? data.originAlias,
			icon: origin?.icon ?? 'icon-wand',
			description: data.originKey,
		};
	}

	#updateDynamicRootQuerySteps(querySteps?: Array<UmbTreePickerDynamicRootQueryStep>) {
		if (!this.data) return;
		this.#sorter.setModel(querySteps?.map((_, index) => index.toString()) ?? []);
		this.data = { ...this.data, ...{ querySteps } };
	}

	#getQueryStepMeta(item: UmbTreePickerDynamicRootQueryStep): { label: string; icon: string; description?: string } {
		const step = this._querySteps.find((step) => step.meta.queryStepAlias === item.alias)?.meta;
		const docTypes = item.anyOfDocTypeKeys?.join(', ');
		const description = docTypes ? this.localize.term('dynamicRoot_queryStepTypes') + docTypes : undefined;

		return {
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
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`${this.#renderButton()} ${this.#renderOrigin()}`;
	}

	#renderButton() {
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
			<uui-ref-list>
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
			</uui-ref-list>

			${this.#renderQuerySteps()} ${this.#renderAddQueryStepButton()}

			<uui-button @click=${this.#clearDynamicRootQuery}>${this.localize.term('buttons_clearSelection')}</uui-button>
		`;
	}

	#renderQuerySteps() {
		if (!this.data?.querySteps) return;
		return html`
			<uui-ref-list id="query-steps">
				${map(this.data.querySteps, (item, index) => this.#renderQueryStep(item, index))}
			</uui-ref-list>
		`;
	}

	#renderQueryStep(item: UmbTreePickerDynamicRootQueryStep, index: number) {
		if (!item.alias) return;
		const step = this.#getQueryStepMeta(item);
		return html`
			<uui-ref-node border standalone data-idx=${index} name=${step.label} detail="${ifDefined(step.description)}">
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
