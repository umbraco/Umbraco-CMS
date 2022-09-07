import '../backoffice/components/backoffice-modal-container.element';
import '../core/services/modal/layouts/content-picker/modal-layout-content-picker.element';
import '../core/context/context-provider.element';
import '../backoffice/editors/shared/editor-layout/editor-layout.element';

import '../backoffice/property-editors/property-editor-icon-picker.element';
import '../core/services/modal/layouts/icon-picker/modal-layout-icon-picker.element';

import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-dialog';

import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import { UmbModalService } from '../core/services/modal';

export default {
	title: 'Editors/Icon Picker',
	component: 'umb-property-editor-icon-picker',
	id: 'icon-picker',
	decorators: [
		(story) =>
			html`
				<uui-icon-registry-essential>
					<umb-context-provider
						style="display: block; padding: 32px;"
						key="umbModalService"
						.value=${new UmbModalService()}>
						${story()}
					</umb-context-provider>
				</uui-icon-registry-essential>
			`,
	],
} as Meta;

export const IconPickerEditor = () => html`<umb-backoffice-modal-container></umb-backoffice-modal-container>
	<umb-property-editor-icon-picker></umb-property-editor-icon-picker>`;

export const IconPickerModalLayout = () => html`<umb-modal-layout-icon-picker></umb-modal-layout-icon-picker>`;
