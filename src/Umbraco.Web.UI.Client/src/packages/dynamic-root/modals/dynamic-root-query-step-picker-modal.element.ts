import { UmbDocumentTypePickerContext } from '../../documents/document-types/components/input-document-type/input-document-type.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, map } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { type UmbTreePickerDynamicRootQueryStep } from '@umbraco-cms/backoffice/components';
import { UmbDynamicRootRepository } from '@umbraco-cms/backoffice/dynamic-root';

@customElement('umb-dynamic-root-query-step-picker-modal')
export class UmbDynamicRootQueryStepPickerModalModalElement extends UmbModalBaseElement {
	#dynamicRootRepository: UmbDynamicRootRepository;

	constructor() {
		super();

		this.#dynamicRootRepository = new UmbDynamicRootRepository(this);
	}

	// TODO: LK to read up on this: https://lit.dev/docs/components/lifecycle/ [LK]
	protected firstUpdated(): void {
		this.#getDynamicRootQuerySteps();
	}

	async #getDynamicRootQuerySteps() {

		const { data } = await this.#dynamicRootRepository.getQuerySteps();
		console.log('steps', data);
	}

	#close() {
		this.modalContext?.reject();
	}

	#documentTypePickerContext = new UmbDocumentTypePickerContext(this);

	#openCustom() {
		this.#submit({
			alias: 'custom',
			anyOfDocTypeKeys: [],
		});
	}

	#openDocumentTypePicker(alias: string) {
		this.#documentTypePickerContext
			.openPicker({
				hideTreeRoot: true,
			})
			.then(() => {
				const selectedItems = this.#documentTypePickerContext.getSelection();
				this.#submit({
					alias: alias,
					anyOfDocTypeKeys: selectedItems,
				});
			});
	}

	#submit(value: UmbTreePickerDynamicRootQueryStep) {
		this.modalContext?.setValue(value);
		this.modalContext?.submit();
	}

	// TODO: This needs to be replaced with a lookup from the manifests, e.g. new extension type `dynamicRoot` [LK]
	#queryStepButtons = [
		{
			alias: 'NearestAncestorOrSelf',
			title: this.localize.term('dynamicRoot_queryStepNearestAncestorOrSelfTitle'),
			description: this.localize.term('dynamicRoot_queryStepNearestAncestorOrSelfDesc'),
			action: () => this.#openDocumentTypePicker('NearestAncestorOrSelf'),
		},
		{
			alias: 'FurthestAncestorOrSelf',
			title: this.localize.term('dynamicRoot_queryStepFurthestAncestorOrSelfTitle'),
			description: this.localize.term('dynamicRoot_queryStepFurthestAncestorOrSelfDesc'),
			action: () => this.#openDocumentTypePicker('FurthestAncestorOrSelf'),
		},
		{
			alias: 'NearestDescendantOrSelf',
			title: this.localize.term('dynamicRoot_queryStepNearestDescendantOrSelfTitle'),
			description: this.localize.term('dynamicRoot_queryStepNearestDescendantOrSelfDesc'),
			action: () => this.#openDocumentTypePicker('NearestDescendantOrSelf'),
		},
		{
			alias: 'FurthestDescendantOrSelf',
			title: this.localize.term('dynamicRoot_queryStepFurthestDescendantOrSelfTitle'),
			description: this.localize.term('dynamicRoot_queryStepFurthestDescendantOrSelfDesc'),
			action: () => this.#openDocumentTypePicker('FurthestDescendantOrSelf'),
		},
		// TODO: Remove `custom` once the above are implemented. [LK]
		{
			alias: 'custom',
			title: this.localize.term('dynamicRoot_queryStepCustomTitle'),
			description: this.localize.term('dynamicRoot_queryStepCustomDesc'),
			action: () => this.#openCustom(),
		},
	];

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('dynamicRoot_pickDynamicRootQueryStepTitle')}">
				<div id="main">
					<uui-box>
						${map(
							this.#queryStepButtons,
							(btn) => html`
								<uui-button @click=${btn.action} look="placeholder" label="${btn.title}">
									<h3>${btn.title}</h3>
									<p>${btn.description}</p>
								</uui-button>
							`,
						)}
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="default" label="${this.localize.term('general_close')}"></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-box > uui-button {
				display: block;
				--uui-button-content-align: flex-start;
			}

			uui-box > uui-button:not(:last-of-type) {
				margin-bottom: var(--uui-size-space-5);
			}

			h3,
			p {
				text-align: left;
			}
		`,
	];
}

export default UmbDynamicRootQueryStepPickerModalModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dynamic-root-query-step-picker-modal': UmbDynamicRootQueryStepPickerModalModalElement;
	}
}
