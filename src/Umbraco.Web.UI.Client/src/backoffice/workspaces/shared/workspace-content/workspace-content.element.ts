import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { distinctUntilChanged } from 'rxjs';
import type { UmbWorkspaceNodeContext } from '../workspace-context/workspace-node.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { DocumentDetails, MediaDetails } from '@umbraco-cms/models';

import '../workspace-entity/workspace-entity.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/edit/workspace-view-content-edit.element';
import './views/info/workspace-view-content-info.element';
import type { UmbNodeStoreBase } from '@umbraco-cms/stores/store';

type ContentTypeTypes = DocumentDetails | MediaDetails;

@customElement('umb-workspace-content')
export class UmbWorkspaceContentElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				margin: 0 var(--uui-size-layout-1);
				flex:1 1 auto;
			}
			#name-input {
				width: 100%;
				height: 100%;/** I really don't know why this fixes the border colliding with variant-selector-toggle, but lets this solution for now */
			}

			#variant-selector-toggle {
				white-space: nowrap;
			}
			#variant-selector-popover {
				display: block;
			}
			#variant-selector-dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
			}

			#footer {
				margin: 0 var(--uui-size-layout-1);
			}
		`,
	];

	// TODO: is this used for anything?
	@property()
	alias!: string;

	// TODO: use a NodeDetails type here:
	@state()
	_content?: ContentTypeTypes;

	private _workspaceContext?: UmbWorkspaceNodeContext<ContentTypeTypes, UmbNodeStoreBase<ContentTypeTypes>>;


	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (instance) => {
			this._workspaceContext = instance;
			this._observeWorkspace();
		});

		this.addEventListener('property-value-change', this._onPropertyValueChange);
	}


	private async _observeWorkspace() {
		if (!this._workspaceContext) return;

		this.observe<ContentTypeTypes>(this._workspaceContext.data.pipe(distinctUntilChanged()), (data) => {
			this._content = data;
		});
	}

	private _onPropertyValueChange = (e: Event) => {
		const target = e.composedPath()[0] as any;

		// TODO: Set value.
		const property = this._content?.properties.find((x) => x.alias === target.alias);
		if (property) {
			this._setPropertyValue(property.alias, target.value);
		} else {
			console.error('property was not found', target.alias);
		}
	};

	// TODO: How do we ensure this is a change of this document and not nested documents? Should the event be stopped at this spot at avoid such.
	private _setPropertyValue(alias: string, value: unknown) {
		this._content?.data.forEach((data) => {
			if (data.alias === alias) {
				data.value = value;
			}
		});
	}

	private _onSaveAndPublish() {
		this._onSave();
	}

	private _onSave() {
		// TODO: What if store is not available, what if content is not loaded... or workspace is not there jet?
		if (this._workspaceContext) {
			this._workspaceContext.save()
		}
	}

	private _onSaveAndPreview() {
		this._onSave();
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._workspaceContext?.update({ name: target.value });
			}
		}
	}

	private _toggleVariantSelector() {
		this._variantSelectorIsOpen = !this._variantSelectorIsOpen;
	}

	@state()
	private _variantSelectorIsOpen = false;

	private _close() {
		this._variantSelectorIsOpen = false;
	}

	render() {
		return html`
			<umb-workspace-entity alias=${this.alias}>
				<div id="header" slot="header">
					<uui-input id="name-input" .value=${this._content?.name} @input="${this._handleInput}">
						<!-- Implement Variant Selector -->
						${this._content && this._content.variants?.length > 0
							? html`
									<div slot="append">
										<uui-button id="variant-selector-toggle" @click=${this._toggleVariantSelector}>
											English (United States)
											<uui-caret></uui-caret>
										</uui-button>
									</div>
							  `
							: nothing}
					</uui-input>

					<!-- Implement Variant Selector -->
					<!-- TODO: Refactor Variant Selector into its own component -->
					${this._content && this._content.variants?.length > 0
						? html`
								<uui-popover id="variant-selector-popover" .open=${this._variantSelectorIsOpen} @close=${this._close}>
									<div id="variant-selector-dropdown" slot="popover">
										<uui-scroll-container>
											<ul>
												<li>Implement variants</li>
											</ul>
										</uui-scroll-container>
									</div>
								</uui-popover>
						  `
						: nothing}
				</div>

				<div id="footer" slot="footer">Breadcrumbs</div>
				<!-- TODO: convert document workspace actions to extensions: -->
				<uui-button slot="actions" @click=${this._onSaveAndPreview} label="Save and preview"></uui-button>
				<uui-button slot="actions" @click=${this._onSave} look="secondary" label="Save"></uui-button>
				<uui-button
					slot="actions" 
					@click=${this._onSaveAndPublish}
					look="primary"
					color="positive"
					label="Save and publish"></uui-button>
			</umb-workspace-entity>
		`;
	}
}

export default UmbWorkspaceContentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-content': UmbWorkspaceContentElement;
	}
}
