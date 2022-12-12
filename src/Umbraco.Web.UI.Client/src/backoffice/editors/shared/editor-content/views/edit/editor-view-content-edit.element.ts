import { html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbNodeContext } from '../../node.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { ContentProperty, ContentPropertyData, DocumentDetails, MediaDetails } from '@umbraco-cms/models';

import '../../../../../components/node-property/node-property.element';

@customElement('umb-editor-view-content-edit')
export class UmbEditorViewContentEditElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles];

	@state()
	_properties: ContentProperty[] = [];

	@state()
	_data: ContentPropertyData[] = [];

	private _nodeContext?: UmbNodeContext;

	constructor() {
		super();

		this.consumeContext('umbNodeContext', (nodeContext) => {
			this._nodeContext = nodeContext;
			this._observeContent();
		});
	}

	private _observeContent() {
		if (!this._nodeContext) return;

		this.observe<DocumentDetails | MediaDetails>(this._nodeContext.data.pipe(distinctUntilChanged()), (content) => {
			this._properties = content.properties;
			this._data = content.data;
		});
	}

	render() {
		return html`
			<uui-box>
				${this._properties.map(
					(property: ContentProperty) => html`
						<umb-node-property
							.property=${property}
							.value=${this._data.find((data) => data.alias === property.alias)?.value}></umb-node-property>
					`
				)}
			</uui-box>
		`;
	}
}

export default UmbEditorViewContentEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-content-edit': UmbEditorViewContentEditElement;
	}
}
