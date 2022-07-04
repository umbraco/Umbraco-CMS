import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbExtensionManifest, UmbExtensionRegistry } from '../../core/extension';

@customElement('umb-editor-extensions')
export class UmbEditorExtensionsElement extends UmbContextConsumerMixin(LitElement) {
	@state()
	private _extensions: Array<UmbExtensionManifest> = [];

	private _extensionRegistry?: UmbExtensionRegistry;
	private _extensionsSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
			this._extensionRegistry = _instance;

			this._extensionsSubscription?.unsubscribe();

			// TODO: Niels: Could we make it easier to unsubscribe? If we invented a Pattern/Mixin/class ala Lit-Controllers we could make it auto unsubscribe.
			// ContextConsumers could be turned into single classes which uses the 'Controller' ability to hook into connected and disconnected.
			// Generally that means that a web component must have the ControllerMixin?? and then controllers can easily be attached, they would know about life cycle and thereby be able to unsubscribe on disconnected etc.
			//
			// All code regarding subscription could be boiled down to:
			// OurUmbracoSubscribeMethod(this, this._extensionRegistry.extensions, (extensions) => {}); // uses `this` to append the subscription to the controller array.
			// Or:
			// this.attachSubscription(this._extensionRegistry.extensions, (extensions) => {});
			this._extensionsSubscription = this._extensionRegistry.extensions.subscribe((extensions) => {
				this._extensions = [...extensions]; // TODO: Though, this is a shallow clone, wouldn't we either do a deep clone or no clone at all?
			});

			this._extensionsSubscription = this._extensionRegistry.extensionsOfType('section').subscribe((sections) => {
				// In this callback sections are typed. Example meta.weight...
				console.log(sections[0].meta.weight);
			});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._extensionsSubscription?.unsubscribe();
	}

	render() {
		return html`
			<uui-box headline="Extensions">
				<uui-table>
					<uui-table-head>
						<uui-table-head-cell>Type</uui-table-head-cell>
						<uui-table-head-cell>Name</uui-table-head-cell>
						<uui-table-head-cell>Alias</uui-table-head-cell>
					</uui-table-head>

					${this._extensions.map(
						(extension) => html`
							<uui-table-row>
								<uui-table-cell>${extension.type}</uui-table-cell>
								<uui-table-cell>${extension.name}</uui-table-cell>
								<uui-table-cell>${extension.alias}</uui-table-cell>
							</uui-table-row>
						`
					)}
				</uui-table>
			</uui-box>
		`;
	}
}

export default UmbEditorExtensionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-extensions': UmbEditorExtensionsElement;
	}
}
