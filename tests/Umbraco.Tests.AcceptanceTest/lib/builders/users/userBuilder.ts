export class UserBuilder {
  email: string;
  kind: string;
  name: string;
  userGroupIds: {id: string}[];
  userName: string;

  constructor() {
    this.userGroupIds = [];
  }

  withEmail(email: string) {
    this.email = email;
    return this;
  }

  withName(name: string) {
    this.name = name;
    return this;
  }

  withKind(kind: string) {
    this.kind = kind;
    return this;
  }

  addUserGroupId(userGroupId: string) {
    this.userGroupIds.push({id: userGroupId});
    return this;
  }

  withUsername(userName: string) {
    this.userName = userName;
    return this;
  }

  build() {
    return {
      email: this.email || '',
      name: this.name || this.email,
      kind: this.kind || 'Default',
      userGroupIds: this.userGroupIds,
      userName: this.userName || this.email,
    };
  }
}
