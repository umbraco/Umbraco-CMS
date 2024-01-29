import { UmbDocumentTypePickerContext } from '../../documents/document-types/components/input-document-type/input-document-type.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, map, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbTreePickerDynamicRootQueryStep } from '@umbraco-cms/backoffice/components';
import { type ManifestDynamicRootQueryStep, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-dynamic-root-query-step-picker-modal')
export class UmbDynamicRootQueryStepPickerModalModalElement extends UmbModalBaseElement {
	@state()
	private _querySteps: Array<ManifestDynamicRootQueryStep> = [];

	#documentTypePickerContext = new UmbDocumentTypePickerContext(this);

	constructor() {
		super();

		this.observe(
			umbExtensionsRegistry.byType('dynamicRootQueryStep'),
			(querySteps: Array<ManifestDynamicRootQueryStep>) => {
				this._querySteps = querySteps;
			},
		);
	}

	#choose(item: ManifestDynamicRootQueryStep) {
		this.#openDocumentTypePicker(item.meta.queryStepAlias);
	}

	#close() {
		this.modalContext?.reject();
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

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('dynamicRoot_pickDynamicRootQueryStepTitle')}">
				<div id="main">
					<uui-box>
						${map(
							this._querySteps,
							(item) => html`
								<uui-button @click=${() => this.#choose(item)} look="placeholder" label="${ifDefined(item.meta.label)}">
									<h3>${item.meta.label}</h3>
									<p>${item.meta.description}</p>
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
