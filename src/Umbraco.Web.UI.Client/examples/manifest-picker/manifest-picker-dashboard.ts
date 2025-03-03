import { css, html, customElement, state, when, nothing } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbInputManifestElement } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

// eslint-disable-next-line local-rules/enforce-umb-prefix-on-element-name
@customElement('example-manifest-picker-dashboard')
// eslint-disable-next-line local-rules/enforce-element-suffix-on-element-class-name, local-rules/umb-class-prefix
export class ExampleManifestPickerDashboard extends UmbLitElement {
	#options: Array<Option> = [];

	@state()
	private _selectedExtensionType: string = 'collectionView';

	@state()
	private _selectedManifest: string = '';

	constructor() {
		super();

		this.observe(umbExtensionsRegistry.extensions, (extensions) => {
			const types = [...new Set(extensions.map((x) => x.type))];
			this.#options = types.sort().map((x) => ({
				name: x,
				value: x,
				selected: x === this._selectedExtensionType,
			}));
		});
	}

	#onSelect(event: UUISelectEvent) {
		this._selectedManifest = '';
		this._selectedExtensionType = event.target.value as string;
	}

	#onChange(event: { target: UmbInputManifestElement }) {
		const selectedManifest = event.target.value;
		this._selectedManifest = selectedManifest?.value ?? '';
	}

	override render() {
		return html`
			<uui-box>
				<umb-property-layout label="Select a extension type...">
					<uui-select
						slot="editor"
						label="Select type..."
						placeholder="Select type..."
						.options=${this.#options}
						@change=${this.#onSelect}></uui-select>
				</umb-property-layout>
				${when(
					this._selectedExtensionType,
					() => html`
						<umb-property-layout label="Selected extension type" description=${this._selectedExtensionType}>
							<div slot="editor">
								<umb-input-manifest
									.extensionType=${this._selectedExtensionType}
									@change=${this.#onChange}></umb-input-manifest>
							</div>
						</umb-property-layout>
					`,
					() => nothing,
				)}
				${when(
					this._selectedManifest,
					() => html`
						<umb-property-layout label="Selected manifest">
							<div slot="editor">
								<code>${this._selectedManifest}</code>
							</div>
						</umb-property-layout>
					`,
					() => nothing,
				)}
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export default ExampleManifestPickerDashboard;

declare global {
	interface HTMLElementTagNameMap {
		'example-manifest-picker-dashboard': ExampleManifestPickerDashboard;
	}
}
