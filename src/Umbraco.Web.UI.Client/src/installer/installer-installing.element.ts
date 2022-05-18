import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import { postInstall } from '../api/fetcher';

@customElement('umb-installer-installing')
export class UmbInstallerInstalling extends LitElement {
  static styles: CSSResultGroup = [
    css`
      uui-input,
      uui-input-password {
        width: 100%;
      }

      #log {
        border: 1px solid #b3b3b3;
        margin-top: 16px;
        padding: 8px 12px;
        background: #f1f1f1;
        color: #282828;
      }

      #buttons {
        display: flex;
      }

      #button-install {
        margin-left: auto;
      }
    `,
  ];

  private _handleSubmit = (e: SubmitEvent) => {
    e.preventDefault();

    const form = e.target as HTMLFormElement;
    if (!form) return;

    const isValid = form.checkValidity();
    if (!isValid) return;

    const formData = new FormData(form);

    const name = formData.get('name') as string;
    const email = formData.get('email') as string;
    const password = formData.get('password') as string;
    const news = formData.has('news');

    this._install(name, email, password, news);
  };

  private async _install(name: string, email: string, password: string, news: boolean) {
    console.log('Installing', name, email, password, news);

    try {
      await postInstall({});

      // TODO: Change to redirect when router has been added.
      this.dispatchEvent(new CustomEvent('install', { bubbles: true, composed: true }));
    } catch (error) {
      console.log(error);
    }
  }

  render() {
    return html` <div class="uui-text">
      <h1 class="uui-h3">Installing Umbraco</h1>
      <p>Almost there! In a few moments your Umbraco adventure begins!</p>
      <uui-progress-bar progress="50"></uui-progress-bar>
      <div id="log">
        struct group_info init_groups = { .usage = ATOMIC_INIT(2) }; struct group_info *groups_alloc(int gidsetsize){
        struct group_info *group_info; int nblocks; int i; nblocks = (gidsetsize + NGROUPS_PER_BLOCK - 1) /
        NGROUPS_PER_BLOCK; /* Make sure we always allocate at least one indirect block pointer */ nblocks = nblocks ? :
        1; group_info = kmalloc(sizeof(*group_info) + nblocks*sizeof(gid_t *), GFP_USER); if (!group_info) return NULL;
        group_info->ngroups = gidsetsize; group_info->nblocks = nblocks; atomic_set(&grou|
      </div>
    </div>`;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-installer-installing': UmbInstallerInstalling;
  }
}
