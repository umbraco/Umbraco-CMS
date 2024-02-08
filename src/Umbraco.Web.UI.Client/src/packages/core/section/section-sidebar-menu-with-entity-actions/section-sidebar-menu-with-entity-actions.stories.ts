import type { Meta, Story } from '@storybook/web-components';
import type { UmbSectionSidebarMenuWithEntityActionsElement } from './section-sidebar-menu-with-entity-actions.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './section-sidebar-menu-with-entity-actions.element.js';

export default {
	title: 'Sections/Shared/Section Sidebar Menu With Entity Actions',
	component: 'umb-section-sidebar-menu-with-entity-actions',
	id: 'umb-section-sidebar-menu-with-entity-actions',
} as Meta;

export const AAAOverview: Story<UmbSectionSidebarMenuWithEntityActionsElement> = () =>
	html` <umb-section-sidebar-menu-with-entity-actions></umb-section-sidebar-menu-with-entity-actions>`;
AAAOverview.storyName = 'Overview';
