import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ITEM_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-input-manifest')
export class UmbInputManifestElement extends UmbLitElement {
	#extensions: Array<typeof umbExtensionsRegistry.MANIFEST_TYPES> = [];

	#extensionType?: string;
	@property({ type: String, attribute: 'extension-type' })
	public set extensionType(value: string | undefined) {
		this.#extensionType = value;
		this.#observeExtensions();
	}
	public get extensionType(): string | undefined {
		return this.#extensionType;
	}

	@property({ attribute: false })
	value?: typeof UMB_ITEM_PICKER_MODAL.VALUE;

	@property({ type: Number })
	max = Infinity;

	#observeExtensions() {
		if (!this.#extensionType) return;
		this.observe(umbExtensionsRegistry.byType(this.#extensionType), (extensions) => {
			this.#extensions = extensions.sort((a, b) => a.type.localeCompare(b.type) || a.alias.localeCompare(b.alias));
		});
	}

	async #onClick() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_ITEM_PICKER_MODAL, {
			data: {
				headline: `${this.localize.term('general_choose')}...`,
				items: this.#extensions
					.filter((ext) => ext.type === this.extensionType)
					.map((ext) => ({
						label: ext.name,
						value: ext.alias,
						description: ext.alias,
						icon: (ext as any).meta?.icon, // HACK: Ugly way to get the icon. [LK]
					})),
			},
		});

		const modalValue = await modalContext.onSubmit();

		if (!modalValue) return;

		this.value = modalValue;

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-button
				label=${this.localize.term('general_choose')}
				look="placeholder"
				color="default"
				@click=${this.#onClick}></uui-button>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbInputManifestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-manifest': UmbInputManifestElement;
	}
}
