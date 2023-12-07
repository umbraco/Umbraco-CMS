import { html, customElement, property, css, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

export type UmbCrop = {
	alias: string;
	width: number;
	height: number;
};

/**
 * @element umb-property-editor-ui-image-crops-configuration
 */
@customElement('umb-property-editor-ui-image-crops-configuration')
export class UmbPropertyEditorUIImageCropsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	//TODO MAKE TYPE
	@property({ attribute: false })
	value: UmbCrop[] = [];

	@state()
	editCropAlias = '';

	#onRemove(alias: string) {
		this.value = [...this.value.filter((item) => item.alias !== alias)];
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	#onEdit(crop: UmbCrop) {
		this.editCropAlias = crop.alias;

		const form = this.shadowRoot?.querySelector('form') as HTMLFormElement;
		if (!form) return;

		const alias = form.querySelector('#alias') as HTMLInputElement;
		const width = form.querySelector('#width') as HTMLInputElement;
		const height = form.querySelector('#height') as HTMLInputElement;

		if (!alias || !width || !height) return;

		alias.value = crop.alias;
		width.value = crop.width.toString();
		height.value = crop.height.toString();
	}

	#onEditCancel() {
		this.editCropAlias = '';
	}

	#onSubmit(event: Event) {
		event.preventDefault();
		const form = event.target as HTMLFormElement;
		if (!form) return;

		if (!form.checkValidity()) return;

		const formData = new FormData(form);

		const alias = formData.get('alias') as string;
		const width = formData.get('width') as string;
		const height = formData.get('height') as string;

		if (!alias || !width || !height) return;
		if (!this.value) this.value = [];

		const newCrop = {
			alias,
			width: parseInt(width),
			height: parseInt(height),
		};

		if (this.editCropAlias) {
			const index = this.value.findIndex((item) => item.alias === this.editCropAlias);
			if (index === -1) return;

			const temp = [...this.value];
			temp[index] = newCrop;
			this.value = [...temp];
			this.editCropAlias = '';
		} else {
			this.value = [...this.value, newCrop];
		}
		this.dispatchEvent(new CustomEvent('property-value-change'));

		form.reset();
	}

	#renderActions() {
		return this.editCropAlias
			? html`<uui-button @click=${this.#onEditCancel}>Cancel</uui-button>
					<uui-button look="secondary" type="submit" label="Save"></uui-button>`
			: html`<uui-button look="secondary" type="submit" label="Add"></uui-button>`;
	}

	render() {
		if (!this.value) this.value = [];

		return html`
			<uui-form>
				<form @submit=${this.#onSubmit}>
					<div class="input">
						<uui-label for="alias">Alias</uui-label>
						<uui-input label="Alias" id="alias" name="alias" type="text" autocomplete="false" value=""></uui-input>
					</div>
					<div class="input">
						<uui-label for="width">Width</uui-label>
						<uui-input label="Width" id="width" name="width" type="number" autocomplete="false" value="">
							<span class="append" slot="append">px</span>
						</uui-input>
					</div>
					<div class="input">
						<uui-label for="height">Height</uui-label>
						<uui-input label="Height" id="height" name="height" type="number" autocomplete="false" value="">
							<span class="append" slot="append">px</span>
						</uui-input>
					</div>
					${this.#renderActions()}
				</form>
			</uui-form>
			<div class="crops">
				${repeat(
					this.value,
					(item) => item.alias,
					(item) => html`
						<div class="crop">
							<span class="crop-drag">+</span>
							<span class="crop-alias">${item.alias}</span>
							<span class="crop-size">(${item.width} x ${item.height}px)</span>
							<div class="crop-actions">
								<uui-button label="Edit" @click=${() => this.#onEdit(item)}>Edit</uui-button>
								<uui-button label="Remove" color="danger" @click=${() => this.#onRemove(item.alias)}>Remove</uui-button>
							</div>
						</div>
					`,
				)}
			</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				max-width: 500px;
				display: block;
			}
			.crops {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
				margin-top: var(--uui-size-space-4);
			}
			.crop {
				display: flex;
				align-items: center;
				background: var(--uui-color-background);
			}
			.crop-drag {
				cursor: grab;
				padding-inline: var(--uui-size-space-4);
				color: var(--uui-color-disabled-contrast);
				font-weight: bold;
			}
			.crop-alias {
				font-weight: bold;
			}
			.crop-size {
				font-size: 0.9em;
				padding-inline: var(--uui-size-space-4);
			}
			.crop-actions {
				display: flex;
				margin-left: auto;
			}
			form {
				display: flex;
				gap: var(--uui-size-space-2);
			}
			.input {
				display: flex;
				flex-direction: column;
			}
			.append {
				padding-inline: var(--uui-size-space-4);
				background: var(--uui-color-disabled);
				border-left: 1px solid var(--uui-color-border);
				color: var(--uui-color-disabled-contrast);
				font-size: 0.8em;
				display: flex;
				align-items: center;
			}
		`,
	];
}

export default UmbPropertyEditorUIImageCropsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-crops-configuration': UmbPropertyEditorUIImageCropsConfigurationElement;
	}
}
