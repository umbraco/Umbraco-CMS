import {
	UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL,
	UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL,
} from '@umbraco-cms/backoffice/dynamic-root';
import { html, css, customElement, property, ifDefined, map } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbTreePickerDynamicRoot, UmbTreePickerDynamicRootQueryStep } from '@umbraco-cms/backoffice/components';
import type { UmbModalContext, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { type UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

const SORTER_CONFIG: UmbSorterConfig<string> = {
	compareElementToModel: (element, model) => {
		return element.getAttribute('data-idx') === model;
	},
	querySelectModelToElement: () => null,
	identifier: 'Umb.SorterIdentifier.InputDocumentPickerRoot',
	itemSelector: 'uui-ref-node',
	containerSelector: '#query-steps',
};

@customElement('umb-input-document-picker-root')
export class UmbInputDocumentPickerRootElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@property({ attribute: false })
	data?: UmbTreePickerDynamicRoot | undefined;

	private _modalContext?: UmbModalManagerContext;

	#openModal?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this._modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		this.#updateQuerySteps(this.data?.querySteps);
	}

	#sorter = new UmbSorterController(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			if (this.data && this.data.querySteps) {
				const steps = [...this.data.querySteps];
				const querySteps = model.map((index) => steps[parseInt(index)]);
				this.#updateQuerySteps(querySteps);
				this.dispatchEvent(new UmbChangeEvent());
			}
		},
	});

	#openDynamicRootOriginPicker() {
		this.#openModal = this._modalContext?.open(UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL, {});
		this.#openModal?.onSubmit().then((data) => {
			this.data = { ...this.data, ...data };
			this.dispatchEvent(new UmbChangeEvent());
		});
	}

	#openDynamicRootQueryStepPicker() {
		this.#openModal = this._modalContext?.open(UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL, {});
		this.#openModal?.onSubmit().then((step) => {
			if (this.data) {
				const querySteps = [...(this.data.querySteps ?? []), step];
				this.#updateQuerySteps(querySteps);
				this.dispatchEvent(new UmbChangeEvent());
			}
		});
	}

	#updateQuerySteps(querySteps?: Array<UmbTreePickerDynamicRootQueryStep>) {
		if (!this.data) return;
		this.#sorter.setModel(querySteps?.map((_, index) => index.toString()) ?? []);
		this.data = { ...this.data, ...{ querySteps } };
	}

	// NOTE: Taken from: https://github.com/umbraco/Umbraco-CMS/blob/release-13.0.0/src/Umbraco.Web.UI.Client/src/views/prevalueeditors/treesource.controller.js#L128-L141 [LK]
	#getIconForDynamicRootOrigin(alias?: string) {
		switch (alias) {
			case 'Parent':
				return 'icon-page-up';
			case 'Current':
				return 'icon-document';
			case 'ByKey':
				return 'icon-wand';
			case 'Root':
			case 'Site':
			default:
				return 'icon-home';
		}
	}

	#getNameForDynamicRootOrigin(alias?: string) {
		return this.localize.term(`dynamicRoot_origin${alias}Title`);
	}

	#getIconForDynamicRootQueryStep(alias?: string) {
		switch (alias) {
			case 'NearestAncestorOrSelf':
			case 'FurthestAncestorOrSelf':
				return 'icon-arrow-up';
			case 'NearestDescendantOrSelf':
			case 'FurthestDescendantOrSelf':
				return 'icon-arrow-down';
			default:
				return 'icon-lab';
		}
	}

	#getNameForDynamicRootQueryStep(alias?: string) {
		switch (alias) {
			case 'NearestAncestorOrSelf':
			case 'FurthestAncestorOrSelf':
			case 'NearestDescendantOrSelf':
			case 'FurthestDescendantOrSelf':
				return this.localize.term(`dynamicRoot_queryStep${alias}Title`);
			default:
				return alias;
		}
	}

	#getDescriptionForDynamicRootQueryStep(item: UmbTreePickerDynamicRootQueryStep) {
		const docTypes = item.anyOfDocTypeKeys?.join(', ');
		return docTypes ? this.localize.term('dynamicRoot_queryStepTypes') + docTypes : undefined;
	}

	#removeDynamicRootQueryStep(item: UmbTreePickerDynamicRootQueryStep) {
		if (this.data?.querySteps) {
			const index = this.data.querySteps.indexOf(item);
			if (index !== -1) {
				const querySteps = [...this.data.querySteps];
				querySteps.splice(index, 1);
				this.#updateQuerySteps(querySteps);
				this.dispatchEvent(new UmbChangeEvent());
			}
		}
	}

	#clearDynamicRootQuery() {
		this.data = undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		// TODO: If the old root node ID value is set, then pre-populate the "Specific Node" option. [LK]
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
		if (!this.data) return;
		return html`
			<uui-ref-list>
				<uui-ref-node
					border
					standalone
					name=${this.#getNameForDynamicRootOrigin(this.data.originAlias)}
					detail=${ifDefined(this.data.originKey)}>
					<uui-icon slot="icon" name=${this.#getIconForDynamicRootOrigin(this.data.originAlias)}></uui-icon>
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
		return html`
			<uui-ref-node
				border
				standalone
				data-idx=${index}
				name=${ifDefined(this.#getNameForDynamicRootQueryStep(item.alias))}
				detail="${ifDefined(this.#getDescriptionForDynamicRootQueryStep(item))}">
				<uui-icon slot="icon" name=${this.#getIconForDynamicRootQueryStep(item.alias)}></uui-icon>
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
