import { UmbDocumentDetailRepository } from '../../../repository/index.js';
import type { UmbCreateBlueprintModalData, UmbCreateBlueprintModalValue } from './create-blueprint-modal.token.js';
import { html, customElement, css, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-create-blueprint-modal')
export class UmbCreateBlueprintModalElement extends UmbModalBaseElement<
	UmbCreateBlueprintModalData,
	UmbCreateBlueprintModalValue
> {
	#documentRepository = new UmbDocumentDetailRepository(this);

	#documentUnique = '';

	@state()
	private _documentName = '';

	@state()
	private _blueprintName = '';

	@state()
	private _parentUnique: string | null = null;

	@state()
	private _hasSelectedLocation = false;

	@state()
	private _hasRootAccess?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.hasDocumentBlueprintRootAccess,
				(hasRootAccess) => (this._hasRootAccess = hasRootAccess ?? false),
				'_observeDocumentBlueprintRootAccess',
			);
		});
	}

	#selectableFilter = (item: { unique: string | null }) => item.unique !== null || this._hasRootAccess === true;

	override firstUpdated() {
		this.#documentUnique = this.data?.unique ?? '';
		this.#getDocumentData();
	}

	async #getDocumentData() {
		const { data } = await this.#documentRepository.requestByUnique(this.#documentUnique);
		if (!data) return;

		this._documentName = data.variants[0].name;
		this._blueprintName = data.variants[0].name;
	}

	#onLocationSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this._parentUnique = event.unique ?? null;
		this._hasSelectedLocation = true;
	}

	#onLocationDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this._parentUnique = null;
		this._hasSelectedLocation = false;
	}

	async #handleSave() {
		this.value = {
			name: this._blueprintName,
			parent: this._parentUnique ? { id: this._parentUnique } : null,
		};
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_createblueprint')}>
				<uui-box id="tree-box" headline=${this.localize.term('blueprints_createBlueprintFrom', this._documentName)}>
					<umb-localize key="blueprints_blueprintDescription"></umb-localize>
					<umb-property-layout label=${this.localize.term('general_name')} orientation="vertical">
						<div slot="editor">
							<uui-input
								id="name"
								label="name"
								.value=${this._blueprintName}
								@input=${(e: UUIInputEvent) => (this._blueprintName = e.target.value as string)}></uui-input>
						</div>
					</umb-property-layout>
					<umb-property-layout label=${this.localize.term('general_choose')} orientation="vertical" mandatory>
						${when(
							this._hasRootAccess !== undefined,
							() => html`
								<umb-tree
									slot="editor"
									alias="Umb.Tree.DocumentBlueprint"
									.props=${{
										hideTreeItemActions: true,
										foldersOnly: true,
										selectableFilter: this.#selectableFilter,
									}}
									@selected=${this.#onLocationSelected}
									@deselected=${this.#onLocationDeselected}>
								</umb-tree>
							`,
						)}
					</umb-property-layout>
				</uui-box>
				<uui-button
					slot="actions"
					id="close"
					label=${this.localize.term('general_close')}
					@click="${this.#handleClose}"></uui-button>
				<uui-button
					slot="actions"
					id="save"
					look="primary"
					color="positive"
					label=${this.localize.term('buttons_save')}
					?disabled=${!this._blueprintName.trim() || !this._hasSelectedLocation}
					@click="${this.#handleSave}"></uui-button>
			</umb-body-layout>
		`;
	}

	#handleClose() {
		this.modalContext?.reject();
	}

	static override styles = [
		UmbTextStyles,
		css`
			strong,
			uui-label,
			uui-input {
				width: 100%;
			}

			uui-label {
				margin-top: var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbCreateBlueprintModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-blueprint-modal': UmbCreateBlueprintModalElement;
	}
}
