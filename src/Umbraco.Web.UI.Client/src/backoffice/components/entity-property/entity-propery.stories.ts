import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbEntityPropertyElement } from './entity-property.element';
import './entity-property.element';

export default {
	title: 'Components/Entity Property',
	component: 'umb-entity-property',
	id: 'umb-entity-property',
} as Meta;

export const AAAOverview: Story<UmbEntityPropertyElement> = () =>
	html` <umb-entity-property
		label="Property"
		description="Description"
		property-editor-ui-alias="Umb.PropertyEditorUI.Text"
		value="Hello"></umb-entity-property>`;
AAAOverview.storyName = 'Overview';
