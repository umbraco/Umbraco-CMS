import { Meta, StoryObj } from '@storybook/web-components';
import './header-app-button.element';
import type { UmbHeaderAppButton } from './header-app-button.element';

const meta: Meta<UmbHeaderAppButton> = {
    title: 'Components/Header App Button',
    component: 'umb-header-app-button'
};
  
export default meta;
type Story = StoryObj<UmbHeaderAppButton>;
  
export const Overview: Story = {
    args: {
        manifest: {
            name: 'Some Manifest',
            alias: 'someManifestAlias',
            type: 'headerApp',
            meta: {
                label: 'Some Header',
                icon: 'umb:home',
                pathname: '/some/path'
            }
        }
    }
};