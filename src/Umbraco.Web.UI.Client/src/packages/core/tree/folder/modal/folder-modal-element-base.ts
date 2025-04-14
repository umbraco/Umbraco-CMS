import { css, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbFolderModel } from '../types.js';

export abstract class UmbFolderModalElementBase<
	FolderModalDataType extends { folderRepositoryAlias: string },
	FolderModalValueType extends { folder: UmbFolderModel },
> extends UmbModalBaseElement<FolderModalDataType, FolderModalValueType> {
	@state()
	_isNew = false;

	folderRepository?: UmbDetailRepository<UmbFolderModel>;

	override connectedCallback(): void {
		super.connectedCallback();
		this.#observeRepository();
	}

	#observeRepository() {
		if (!this.data?.folderRepositoryAlias) throw new Error('A folder repository alias is required');

		new UmbExtensionApiInitializer(
			this,
			umbExtensionsRegistry,
			this.data.folderRepositoryAlias,
			[this],
			(permitted, ctrl) => {
				this.folderRepository = permitted ? (ctrl.api as UmbDetailRepository<UmbFolderModel>) : undefined;
				if (this.folderRepository) {
					this.init();
				}
			},
		);
	}

	abstract init(): void;
	abstract onFormSubmit({ name }: { name: string }): void;

	async #onSubmit(event: SubmitEvent) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);
		const name = formData.get('name') as string;

		this.onFormSubmit({ name });
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term(this._isNew ? 'actions_folderCreate' : 'actions_folderRename')}>
				<uui-box>
					<uui-form>
						<form id="FolderForm" @submit="${this.#onSubmit}">
							<uui-form-layout-item>
								<uui-label id="nameLabel" for="name" slot="label" required>
									<umb-localize key="create_enterFolderName">Enter folder name</umb-localize>
								</uui-label>
								<uui-input
									type="text"
									id="name"
									name="name"
									.label=${this.localize.term('create_enterFolderName')}
									.value="${this.value?.folder?.name || ''}"
									required
									${umbFocus()}></uui-input>
							</uui-form-layout-item>
						</form>
					</uui-form>
				</uui-box>

				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click="${this._rejectModal}"></uui-button>
				<uui-button
					form="FolderForm"
					type="submit"
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label=${this.localize.term(this._isNew ? 'actions_folderCreate' : 'actions_folderRename')}></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#name {
				width: 100%;
			}
		`,
	];
}
