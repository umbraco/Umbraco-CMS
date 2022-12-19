import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { ContentProperty, ContentPropertyData, DocumentDetails, MediaDetails } from '@umbraco-cms/models';

import '../../../../../components/content-property/content-property.element';
import { UmbWorkspaceDocumentContext } from 'src/backoffice/workspaces/document/workspace-document.context';

@customElement('umb-workspace-view-content-edit')
export class UmbWorkspaceViewContentEditElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display:block;
				margin: var(--uui-size-layout-1);
			}
		`
	];

	@state()
	_properties: ContentProperty[] = [];

	@state()
	_data: ContentPropertyData[] = [];

	private _workspaceContext?: UmbWorkspaceDocumentContext;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (workspaceContext) => {
			this._workspaceContext = workspaceContext;

			console.log("umbWorkspaceContext got", workspaceContext)
			this._observeContent();
		});
	}

	private _observeContent() {
		if (!this._workspaceContext) return;

		this.observe<DocumentDetails | MediaDetails>(this._workspaceContext.data.pipe(distinctUntilChanged()), (content) => {
			console.log("observe data", content)
			this._properties = content.properties;
			this._data = content.data;
		});
	}

	render() {
		return html`
			<uui-box>
				${this._properties?.map(
					(property: ContentProperty) => html`
						<umb-content-property
							.property=${property}
							.value=${this._data.find((data) => data.alias === property.alias)?.value}></umb-content-property>
					`
				)}
			</uui-box>
		`;
	}
}

export default UmbWorkspaceViewContentEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-content-edit': UmbWorkspaceViewContentEditElement;
	}
}