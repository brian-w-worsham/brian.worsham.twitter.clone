export class User {
  constructor(
    public userName: string,
    public email: string,
    public password: string,
    public userRole: string,
    public bio?: string,
    public profilePictureUrl?: string,
    public id?: number
  ) {}
}
