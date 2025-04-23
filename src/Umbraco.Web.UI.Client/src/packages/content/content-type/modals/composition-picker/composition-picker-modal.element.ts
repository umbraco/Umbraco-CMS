import type {
	UmbContentTypeCompositionRepository,
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
} from '../../composition/index.js';
import type {
	UmbCompositionPickerModalData,
	UmbCompositionPickerModalValue,
} from './composition-picker-modal.token.js';
import { css, html, customElement, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

interface CompatibleCompositions {
	path: string;
	compositions: Array<UmbContentTypeCompositionCompatibleModel>;
}

@customElement('umb-composition-picker-modal')
export class UmbCompositionPickerModalElement extends UmbModalBaseElement<
	UmbCompositionPickerModalData,
	UmbCompositionPickerModalValue
> {
	// TODO: Loosen this from begin specific to Document Types, so we can have a general interface for composition repositories. [NL]
	#compositionRepository?: UmbContentTypeCompositionRepository;
	#unique: string | null = null;
	#init?: Promise<void>;

	@state()
	private _references: Array<UmbContentTypeCompositionReferenceModel> = [];

	@state()
	private _compatibleCompositions?: Array<CompatibleCompositions>;

	@state()
	private _selection: Array<string> = [];

	@state()
	private _usedForInheritance: Array<string> = [];

	override connectedCallback() {
		super.connectedCallback();

		const alias = this.data?.compositionRepositoryAlias;
		if (alias) {
			this.#init = new UmbExtensionApiInitializer(this, umbExtensionsRegistry, alias, [this], (permitted, ctrl) => {
				this.#compositionRepository = permitted ? (ctrl.api as UmbContentTypeCompositionRepository) : undefined;
			}).asPromise();
		} else {
			throw new Error('No composition repository alias provided');
		}

		this._selection = this.data?.selection ?? [];
		this._usedForInheritance = this.data?.usedForInheritance ?? [];
		this.modalContext?.setValue({ selection: this._selection });

		const isNew = this.data!.isNew;
		this.#unique = !isNew ? this.data!.unique : null;

		this.#requestReference();
		this.#requestAvailableCompositions();
	}

	protected override async _submitModal() {
		const initSelection = this.data?.selection ?? [];
		const newSelection = this._selection;

		const setA = new Set(newSelection);
		const existingCompositionHasBeenRemoved = !initSelection.every((item) => setA.has(item));

		if (existingCompositionHasBeenRemoved) {
			await umbConfirmModal(this, {
				headline: this.localize.term('general_remove'),
				content: html`<div style="max-width:400px">
					${this.localize.term('contentTypeEditor_compositionRemoveWarning')}
				</div>`,
				confirmLabel: this.localize.term('general_submit'),
				color: 'danger',
			});
		}

		super._submitModal();
	}

	async #requestReference() {
		await this.#init;
		if (!this.#unique || !this.#compositionRepository) return;

		const { data } = await this.#compositionRepository.getReferences(this.#unique);
		this._references = data ?? [];
	}

	async #requestAvailableCompositions() {
		await this.#init;
		if (!this.#compositionRepository) return;

		// Notice isElement is not available on all types that can be composed.
		const isElement = this.data?.isElement ?? undefined;
		const currentPropertyAliases = this.data?.currentPropertyAliases ?? [];

		const { data } = await this.#compositionRepository.availableCompositions({
			unique: this.#unique,
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			// TODO: isElement is not available on all types that can be composed.
			isElement: isElement,
			currentCompositeUniques: this._selection,
			currentPropertyAliases: currentPropertyAliases,
		});

		if (!data) return;

		const folders = Array.from(new Set(data.map((c) => '/' + c.folderPath.join('/'))));
		this._compatibleCompositions = folders.map((path) => ({
			path,
			compositions: data.filter((c) => '/' + c.folderPath.join('/') === path),
		}));
	}

	#onSelectionAdd(unique: string) {
		this._selection = [...this._selection, unique];
		this.modalContext?.setValue({ selection: this._selection });
	}

	#onSelectionRemove(unique: string) {
		this._selection = this._selection.filter((s) => s !== unique);
		this.modalContext?.setValue({ selection: this._selection });
	}

	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('contentTypeEditor_compositions')}">
				${this._references.length ? this.#renderHasReference() : this.#renderAvailableCompositions()}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					${!this._references.length
						? html`
								<uui-button
									label=${this.localize.term('general_submit')}
									look="primary"
									color="positive"
									@click=${this._submitModal}></uui-button>
							`
						: nothing}
				</div>
			</umb-body-layout>
		`;
	}

	#renderHasReference() {
		return html`
			<umb-localize key="contentTypeEditor_compositionInUse">
				This Content Type is used in a composition, and therefore cannot be composed itself.
			</umb-localize>
			<h4>
				<umb-localize key="contentTypeEditor_compositionUsageHeading">Where is this composition used?</umb-localize>
			</h4>
			<umb-localize key="contentTypeEditor_compositionUsageSpecification">
				This composition is currently used in the composition of the following Content Types:
			</umb-localize>
			<div class="reference-list">
				${repeat(
					this._references,
					(item) => item.unique,
					(item) => html`
						<uui-ref-node-document-type
							href=${'/section/settings/workspace/document-type/edit/' + item.unique}
							name=${this.localize.string(item.name)}>
							<umb-icon slot="icon" name=${item.icon}></umb-icon>
						</uui-ref-node-document-type>
					`,
				)}
			</div>
		`;
	}

	#renderAvailableCompositions() {
		if (this._compatibleCompositions) {
			return html`
				<umb-localize key="contentTypeEditor_compositionsDescription">
					Inherit tabs and properties from an existing Document Type. New tabs will be<br />added to the current
					Document Type or merged if a tab with an identical name exists.<br />
				</umb-localize>
				<!--- TODO: Implement search functionality
				<uui-input id="search" placeholder=this.localize.term('placeholders_filter')>
					<uui-icon name="icon-search" slot="prepend"></uui-icon>
				</uui-input> -->
				<div class="compositions-list">
					${repeat(
						this._compatibleCompositions,
						(folder) => folder.path,
						(folder) =>
							html`${this._compatibleCompositions!.length > 1
								? html`<strong><umb-icon name="icon-folder"></umb-icon>${folder.path}</strong>`
								: nothing}
							${this.#renderCompositionsItems(folder.compositions)}`,
					)}
				</div>
			`;
		} else {
			return html`
				<umb-localize key="contentTypeEditor_noAvailableCompositions">
					There are no Content Types available to use as a composition
				</umb-localize>
			`;
		}
	}

	#renderCompositionsItems(compositionsList: Array<UmbContentTypeCompositionCompatibleModel>) {
		return repeat(
			compositionsList,
			(compositions) => compositions.unique,
			(compositions) => {
				const usedForInheritance = this._usedForInheritance.includes(compositions.unique);
				return html`
					<uui-menu-item
						label=${this.localize.string(compositions.name)}
						?selectable=${!usedForInheritance}
						?disabled=${usedForInheritance}
						@selected=${() => this.#onSelectionAdd(compositions.unique)}
						@deselected=${() => this.#onSelectionRemove(compositions.unique)}
						?selected=${this._selection.find((unique) => unique === compositions.unique)}>
						<umb-icon name=${compositions.icon} slot="icon"></umb-icon>
					</uui-menu-item>
				`;
			},
		);
	}

	static override styles = [
		css`
			uui-input {
				margin: var(--uui-size-6) 0;
				display: flex;
				align-items: center;
			}

			#search uui-icon {
				padding-left: var(--uui-size-3);
			}

			.reference-list {
				margin-block: var(--uui-size-3);
				display: grid;
				gap: var(--uui-size-1);
			}

			.compositions-list strong {
				display: flex;
				align-items: center;
				gap: var(--uui-size-3);
			}
		`,
	];
}

export default UmbCompositionPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-composition-picker-modal': UmbCompositionPickerModalElement;
	}
}
